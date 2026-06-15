using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Diagnostics;
using System.IO;            
using Microsoft.Win32;      
using System.Text;          
namespace MatrixInverter
{
    public partial class MainWindow : Window
    {
        private int size = 3;
        private TextBox[,] matrixInputs = new TextBox[0, 0];
        private TextBlock[,] matrixOutputs = new TextBlock[0, 0];

        public MainWindow()
        {
            InitializeComponent();
            CreateGrids(size);
        }

        public MatrixCore MatrixCore { get; set; } = new MatrixCore();

        private void CreateGrids(int n)
        {
            GridIn.Children.Clear();
            GridOut.Children.Clear();
            GridIn.Rows = n; GridIn.Columns = n;
            GridOut.Rows = n; GridOut.Columns = n;

            matrixInputs = new TextBox[n, n];
            matrixOutputs = new TextBlock[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrixInputs[i, j] = new TextBox
                    {
                        Text = "0",
                        Margin = new Thickness(3),
                        FontSize = 14,
                        TextAlignment = TextAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Height = 35,
                        Width = 60,
                        Background = Brushes.White
                    };
                    GridIn.Children.Add(matrixInputs[i, j]);

                    Border b = new Border
                    {
                        Background = Brushes.White,
                        Margin = new Thickness(3),
                        CornerRadius = new CornerRadius(3),
                        Height = 35,
                        Width = 60
                    };
                    matrixOutputs[i, j] = new TextBlock
                    {
                        Text = "-",
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = Brushes.Purple
                    };
                    b.Child = matrixOutputs[i, j];
                    GridOut.Children.Add(b);
                }
            }
        }

        private void BtnRandom_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    double val = Math.Round(rnd.NextDouble() * 10 - 5, 1);
                    if (i == j && val == 0) val = 1;
                    matrixInputs[i, j].Text = val.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        private void BtnCalc_Click(object sender, RoutedEventArgs e)
        {
            double[,] A = new double[size, size];
            try
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        string val = matrixInputs[i, j].Text.Replace(',', '.');
                        A[i, j] = double.Parse(val, CultureInfo.InvariantCulture);
                    }
                }

                Stopwatch sw = Stopwatch.StartNew();
                double[,] inv;
                int iters = 0;

                if (cmbMethod.SelectedIndex == 0) // Гаус-Жордан
                {
                    inv = MatrixCore.InverseGauss(A);
                    iters = 1;
                }
                else // Хотеллінг-Шульц
                {
                    inv = MatrixCore.InverseNewtonSchulz(A, out iters);
                }

                sw.Stop();

                lblTime.Text = $"Час: {sw.Elapsed.TotalMilliseconds:F2} мс";
                lblIters.Text = $"Ітер: {iters}";

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        double val = inv[i, j];
                        if (Math.Abs(val) < 1e-9) val = 0.0;

                        matrixOutputs[i, j].Text = Math.Abs(val) < 0.001 && val != 0
                            ? val.ToString("E2", CultureInfo.InvariantCulture)
                            : val.ToString("F3", CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Математична помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FormatException)
            {
                MessageBox.Show("Перевір формат чисел!", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (matrixOutputs.GetLength(0) == 0 || matrixOutputs[0, 0].Text == "-")
                {
                    MessageBox.Show("Спочатку обчисліть обернену матрицю!", "Увага", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*";
                saveFileDialog.Title = "Зберегти обернену матрицю";
                saveFileDialog.FileName = "InverseMatrix.txt";

                if (saveFileDialog.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < size; i++)
                    {
                        for (int j = 0; j < size; j++)
                        {
                            sb.Append(matrixOutputs[i, j].Text).Append("\t");
                        }
                        sb.AppendLine(); 
                    }

                    File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                    MessageBox.Show("Матрицю успішно збережено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка при збереженні файлу: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void cmbMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lblComp == null) return;

            if (cmbMethod.SelectedIndex == 0)
                lblComp.Text = "Складність: O(n³)";
            else
                lblComp.Text = "Складність: O(k · n³)";
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (size < 10) { size++; txtSize.Text = size.ToString(); CreateGrids(size); }
        }

        private void BtnDown_Click(object sender, RoutedEventArgs e)
        {
            if (size > 2) { size--; txtSize.Text = size.ToString(); CreateGrids(size); }
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            CreateGrids(size);
        }
    }
}