using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Diagnostics;

namespace MatrixInverter
{
    public partial class MainWindow : Window
    {
        private int size = 3;
        // Додано ініціалізацію за замовчуванням, щоб прибрати попередження CS8618
        private TextBox[,] matrixInputs = new TextBox[0, 0];
        private TextBlock[,] matrixOutputs = new TextBlock[0, 0];

        public MainWindow()
        {
            InitializeComponent();
            CreateGrids(size);
        }

        // Властивість для діаграми класів
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
                int iters = 1;

                if (cmbMethod.SelectedIndex == 0) // Гаусс
                {
                    inv = MatrixCore.InverseGauss(A);
                }
                else // Ньютон-Шульц
                {
                    // ВИПРАВЛЕНО CS1615: виклик методу без out, якщо він тепер повертає лише матрицю
                    inv = MatrixCore.InverseNewtonSchulz(A);
                    iters = 50; // Або інше значення ітерацій для виводу
                }

                sw.Stop();

                lblTime.Text = $"Час: {sw.Elapsed.TotalMilliseconds:F2} мс";
                lblIters.Text = $"Ітер: {iters}";

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        matrixOutputs[i, j].Text = Math.Abs(inv[i, j]) < 0.001 && inv[i, j] != 0
                            ? inv[i, j].ToString("E2", CultureInfo.InvariantCulture)
                            : inv[i, j].ToString("F3", CultureInfo.InvariantCulture);
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
                MessageBox.Show("Помилка: " + ex.Message);
            }
        }

        private void BtnUp_Click(object sender, RoutedEventArgs e)
        {
            if (size < 15) { size++; txtSize.Text = size.ToString(); CreateGrids(size); }
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
