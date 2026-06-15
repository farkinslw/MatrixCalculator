using System;

namespace MatrixInverter
{
    public class MatrixCore
    {
        //Г-Ж
        public double[,] InverseGauss(double[,] matrix)
        {
            ValidateSquareMatrix(matrix);

            int n = matrix.GetLength(0);
            double[,] result = Identity(n);
            double[,] temp = (double[,])matrix.Clone();

            double norm = NormInf(temp);
            double epsilon = Math.Max(1e-15, 1e-12 * norm);

            for (int i = 0; i < n; i++)
            {
                int pivotRow = i;
                double maxVal = Math.Abs(temp[i, i]);
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(temp[k, i]) > maxVal)
                    {
                        maxVal = Math.Abs(temp[k, i]);
                        pivotRow = k;
                    }
                }

                if (pivotRow != i)
                {
                    for (int j = 0; j < n; j++)
                    {
                        (temp[i, j], temp[pivotRow, j]) = (temp[pivotRow, j], temp[i, j]);
                        (result[i, j], result[pivotRow, j]) = (result[pivotRow, j], result[i, j]);
                    }
                }

                if (Math.Abs(temp[i, i]) <= epsilon)
                    throw new InvalidOperationException("Матриця вироджена або занадто погано обумовлена. Обернення неможливе.");

                double factor = temp[i, i];

                for (int j = 0; j < n; j++)
                {
                    temp[i, j] /= factor;
                    result[i, j] /= factor;
                }

                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double f = temp[k, i];
                        if (Math.Abs(f) > 1e-16)
                        {
                            for (int j = 0; j < n; j++)
                            {
                                temp[k, j] -= f * temp[i, j];
                                result[k, j] -= f * result[i, j];
                            }
                        }
                    }
                }
            }
            return result;
        }

        // Х-Ш
        public double[,] InverseNewtonSchulz(double[,] matrix, out int iterations, double eps = 1e-6, int maxIterations = 100)
        {
            ValidateSquareMatrix(matrix);

            int n = matrix.GetLength(0);

            double n1 = Norm1(matrix);
            double nInf = NormInf(matrix);

            if (n1 < 1e-15 || nInf < 1e-15)
                throw new InvalidOperationException("Матриця нульова або близька до неї. Метод не може стартувати.");

            double[,] result = MultiplyScalar(Transpose(matrix), 1.0 / (n1 * nInf));

            if (double.IsInfinity(result[0, 0]) || double.IsNaN(result[0, 0]))
                throw new InvalidOperationException("Помилка ініціалізації початкового наближення.");

            double[,] psi = new double[n, n];
            double[,] identity = Identity(n);
            double[,] aByU = new double[n, n];

            double error = double.MaxValue;
            int iteration = 0;

            while (error > eps)
            {
                if (iteration >= maxIterations)
                    throw new InvalidOperationException($"Метод не зійшовся за {maxIterations} ітерацій.");

                MultiplyNonAlloc(matrix, result, aByU);

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        psi[i, j] = identity[i, j] - aByU[i, j];
                    }
                }

                double nextError = NormInf(psi);

                if (iteration > 5 && nextError > error * 2.0)
                    throw new InvalidOperationException("Ітераційний процес розбігається. Метод не підходить для цієї матриці.");

                error = nextError;

                if (error <= eps)
                    break;

                double[,] term = new double[n, n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        term[i, j] = identity[i, j] + psi[i, j];
                    }
                }

                double[,] nextResult = new double[n, n];
                MultiplyNonAlloc(result, term, nextResult);
                result = nextResult;

                iteration++;
            }

            iterations = iteration; 
            return result;
        }


        private void ValidateSquareMatrix(double[,] matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix), "Матриця не може бути null.");

            if (matrix.Rank != 2)
                throw new ArgumentException("Масив повинен бути двовимірним.", nameof(matrix));

            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            if (rows == 0 || cols == 0)
                throw new ArgumentException("Матриця не може бути пустою.");

            if (rows != cols)
                throw new ArgumentException($"Матриця повинна бути квадратною. Розмір: {rows}x{cols}.");
        }

        public double[,] Identity(int n)
        {
            double[,] res = new double[n, n];
            for (int i = 0; i < n; i++) res[i, i] = 1.0;
            return res;
        }

        private void MultiplyNonAlloc(double[,] A, double[,] B, double[,] target)
        {
            int n = A.GetLength(0);
            Array.Clear(target, 0, target.Length);

            for (int i = 0; i < n; i++)
            {
                for (int k = 0; k < n; k++)
                {
                    double aVal = A[i, k];
                    if (Math.Abs(aVal) > 1e-16)
                    {
                        for (int j = 0; j < n; j++)
                        {
                            target[i, j] += aVal * B[k, j];
                        }
                    }
                }
            }
        }

        public double[,] Multiply(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            double[,] res = new double[n, n];
            MultiplyNonAlloc(A, B, res);
            return res;
        }

        public double[,] MultiplyScalar(double[,] A, double k)
        {
            int n = A.GetLength(0);
            double[,] res = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    res[i, j] = A[i, j] * k;
            return res;
        }

        public double[,] Transpose(double[,] A)
        {
            int n = A.GetLength(0);
            double[,] res = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    res[i, j] = A[j, i];
            return res;
        }

        public double Norm1(double[,] A)
        {
            double max = 0;
            int n = A.GetLength(0);
            for (int j = 0; j < n; j++)
            {
                double sum = 0;
                for (int i = 0; i < n; i++) sum += Math.Abs(A[i, j]);
                if (sum > max) max = sum;
            }
            return max;
        }

        public double NormInf(double[,] A)
        {
            double max = 0;
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < n; j++) sum += Math.Abs(A[i, j]);
                if (sum > max) max = sum;
            }
            return max;
        }
    }
}