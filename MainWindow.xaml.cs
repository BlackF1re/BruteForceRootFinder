using System;
using System.Windows;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using OxyPlot.Annotations;
using OxyPlot.Wpf;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BruteForceRootFinder
{
    public partial class MainWindow : Window
    {
        private PlotModel plotModel;

        public MainWindow()
        {
            InitializeComponent();
            plotModel = new PlotModel { Title = "График функции" };
            PlotView.Model = plotModel;
        }

        private void PlotButton_Click(object sender, RoutedEventArgs e)
        {
            plotModel.Series.Clear();
            plotModel.Annotations.Clear();

            var selectedFunction = FunctionComboBox.SelectedItem as ComboBoxItem;
            if (selectedFunction == null) return;

            Func<double, double> function = GetFunction(selectedFunction.Content.ToString());

            var series = new LineSeries { Title = "f(x)" };
            for (double x = -10; x <= 10; x += 0.1)
            {
                series.Points.Add(new DataPoint(x, function(x)));
            }

            plotModel.Series.Add(series);

            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "x", Minimum = -10, Maximum = 10 });
            plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "f(x)", Minimum = -10, Maximum = 10 });

            // Добавляем пунктирные линии для координат (0, 0) в виде аннотаций
            plotModel.Annotations.Add(new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = 0,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
            });

            plotModel.Annotations.Add(new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                X = 0,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
            });

            PlotView.InvalidatePlot(true);
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            // Задаем начальные параметры
            double step = 0.1;
            double initialGuess = 0;
            double range = 10;

            var function = new Func<double, double>(x => x * x - 4);

            // Используем метод поиска корня с визуализацией
            (double? solution, var points) = BruteForceMethodWithSteps(function, initialGuess, step, range);

            // Обновляем график с результатами
            plotModel.Series.Clear();
            plotModel.Annotations.Clear();

            // Рисуем график функции
            var series = new LineSeries { Title = "f(x) = x^2 - 4" };
            for (double x = -10; x <= 10; x += 0.1)
            {
                series.Points.Add(new DataPoint(x, function(x)));
            }
            plotModel.Series.Add(series);

            // Добавляем пунктирные линии для координат (0, 0) в виде аннотаций
            plotModel.Annotations.Add(new LineAnnotation
            {
                Type = LineAnnotationType.Horizontal,
                Y = 0,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
            });

            plotModel.Annotations.Add(new LineAnnotation
            {
                Type = LineAnnotationType.Vertical,
                X = 0,
                Color = OxyColors.Gray,
                LineStyle = LineStyle.Dash,
            });

            // Добавляем шаги поиска
            if (points.Count > 0)
            {
                var stepSeries = new LineSeries { Title = "Шаги", Color = OxyColors.Yellow };
                foreach (var point in points)
                {
                    stepSeries.Points.Add(new DataPoint(point.X, function(point.X)));
                }
                plotModel.Series.Add(stepSeries);
            }

            // Добавляем финальное решение
            if (solution.HasValue)
            {
                var solutionSeries = new LineSeries { Title = "Решение", Color = OxyColors.Red };
                solutionSeries.Points.Add(new DataPoint(solution.Value, function(solution.Value)));
                plotModel.Series.Add(solutionSeries);

                SolutionLabel.Content = $"Решение: {solution.Value}";
            }
            else
            {
                SolutionLabel.Content = "Решение не найдено";
            }

            // Обновляем оси, чтобы учесть новые данные
            plotModel.Axes[0].Minimum = -10;
            plotModel.Axes[0].Maximum = 10;
            plotModel.Axes[1].Minimum = -10;
            plotModel.Axes[1].Maximum = 10;

            PlotView.InvalidatePlot(true);
        }

        private (double? Solution, List<DataPoint> Points) BruteForceMethodWithSteps(Func<double, double> function, double start, double step, double range)
        {
            var points = new List<DataPoint>();
            double x = start;
            double lastValue = function(x);

            points.Add(new DataPoint(x, lastValue));

            for (x += step; x <= start + range; x += step)
            {
                double currentValue = function(x);
                points.Add(new DataPoint(x, currentValue));

                if (lastValue * currentValue < 0)
                {
                    // Найден интервал с изменением знака
                    double solution = x - step + (step * (0 - lastValue) / (currentValue - lastValue));
                    return (solution, points);
                }

                lastValue = currentValue;
            }

            return (null, points);
        }
        private Func<double, double> GetFunction(string functionName)
        {
            switch (functionName)
            {
                case "f1(x) = x^2 - 4":
                    return x => x * x - 4;
                case "f2(x) = x^3 - 2*x - 5":
                    return x => x * x * x - 2 * x - 5;
                case "f3(x) = sin(x) - 0.5":
                    return x => Math.Sin(x) - 0.5;
                case "f4(x) = cos(x) - x":
                    return x => Math.Cos(x) - x;
                case "f5(x) = e^x - 2":
                    return x => Math.Exp(x) - 2;
                default:
                    throw new InvalidOperationException("Unknown function");
            }
        }
    }
}
