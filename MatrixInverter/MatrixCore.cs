using System;

namespace MatrixInverter
{
    public class MatrixCore
    {
        // Метод Гаусса-Йордана
        public double[,] InverseGauss(double[,] matrix)
        {
            int n = matrix.GetLength(0);
            double[,] result = Identity(n);
            double[,] temp = (double[,])matrix.Clone();

            for (int i = 0; i < n; i++)
            {
                // Перевірка на виродженість (якщо головний елемент ~ 0)
                if (Math.Abs(temp[i, i]) < 1e-10)
                    throw new InvalidOperationException("Матриця вироджена. Оберненої не існує.");

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
                        for (int j = 0; j < n; j++)
                        {
                            temp[k, j] -= f * temp[i, j];
                            result[k, j] -= f * result[i, j];
                        }
                    }
                }
            }
            return result;
        }

        // Метод Ньютона-Шульца (ітераційний)
        public double[,] InverseNewtonSchulz(double[,] matrix, double eps = 1e-6)
        {
            int n = matrix.GetLength(0);
            double[,] result = MultiplyScalar(Transpose(matrix), 1.0 / (Norm1(matrix) * NormInf(matrix)));

            // Перевірка на старті (якщо початкове наближення неможливе)
            if (double.IsInfinity(result[0, 0]) || double.IsNaN(result[0, 0]))
                throw new InvalidOperationException("Метод Ньютона-Шульца не збігається для цієї матриці.");

            double error = double.MaxValue;
            while (error > eps)
            {
                double[,] prev = (double[,])result.Clone();
                double[,] identity = Identity(n);
                double[,] mByR = Multiply(matrix, result);

                // Формула: R_{k+1} = R_k * (2I - A*R_k)
                double[,] term = Subtract(MultiplyScalar(identity, 2.0), mByR);
                result = Multiply(result, term);

                error = CalculateError(result, prev);
            }
            return result;
        }

        //  Допоміжні методи 
        public double[,] Identity(int n)
        {
            double[,] res = new double[n, n];
            for (int i = 0; i < n; i++) res[i, i] = 1;
            return res;
        }

        public double[,] Multiply(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            double[,] res = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < n; k++)
                        res[i, j] += A[i, k] * B[k, j];
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

        public double[,] Subtract(double[,] A, double[,] B)
        {
            int n = A.GetLength(0);
            double[,] res = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    res[i, j] = A[i, j] - B[i, j];
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

        private double CalculateError(double[,] A, double[,] B)
        {
            double sum = 0;
            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    sum += Math.Pow(A[i, j] - B[i, j], 2);
            return Math.Sqrt(sum);
        }
    }
}