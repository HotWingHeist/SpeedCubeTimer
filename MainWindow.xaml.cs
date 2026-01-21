using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using SpeedCubeTimer.Services;

namespace SpeedCubeTimer
{
    public partial class MainWindow : Window
    {
        private readonly ITimerService _timerService;
        private readonly IStatisticsService _statisticsService;
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        private PlotModel _plotModel;
        private bool _isRunning = false;
        private bool _spacePressed = false;
        private Stopwatch _holdStopwatch;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize services
            _timerService = new TimerService();
            _statisticsService = new StatisticsService();
            _holdStopwatch = new Stopwatch();
            
            InitializeDispatcherTimer();
            InitializePlot();
            this.PreviewKeyDown += MainWindow_KeyDown;
            this.PreviewKeyUp += MainWindow_KeyUp;
        }

        private void InitializePlot()
        {
            _plotModel = new PlotModel 
            { 
                Title = "Solve Times Over Session",
                Background = OxyColor.FromRgb(38, 38, 38),
                PlotAreaBackground = OxyColor.FromRgb(26, 26, 26),
                TextColor = OxyColor.FromRgb(255, 255, 255)
            };

            // Add axes
            _plotModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Bottom,
                Title = "Solve #",
                Minimum = 0,
                TextColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(100, 100, 100)
            });
            
            _plotModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Left,
                Title = "Time (seconds)",
                Minimum = 0,
                TextColor = OxyColor.FromRgb(255, 255, 255),
                TicklineColor = OxyColor.FromRgb(100, 100, 100)
            });

            // Add line series
            var lineSeries = new LineSeries
            {
                Title = "Solve Times",
                Color = OxyColor.FromRgb(76, 175, 80),
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 12,
                MarkerFill = OxyColor.FromRgb(76, 175, 80),
                TrackerFormatString = "",
                CanTrackerInterpolatePoints = false
            };
            _plotModel.Series.Add(lineSeries);

            TrendPlot.Model = _plotModel;
        }

        private void UpdatePlot()
        {
            var times = _timerService.GetRecordedTimes();
            
            // Remove all series except the line series
            while (_plotModel.Series.Count > 1)
            {
                _plotModel.Series.RemoveAt(_plotModel.Series.Count - 1);
            }
            
            var lineSeries = _plotModel.Series[0] as LineSeries;
            
            if (lineSeries != null)
            {
                lineSeries.Points.Clear();
                
                for (int i = 0; i < times.Count; i++)
                {
                    lineSeries.Points.Add(new DataPoint(i + 1, times[i]));
                }

                // Update axis limits
                if (times.Count > 0)
                {
                    var xAxis = _plotModel.Axes[0];
                    var yAxis = _plotModel.Axes[1];
                    
                    xAxis.Maximum = times.Count + 1;
                    
                    double maxTime = times.Max();
                    double minTime = times.Min();
                    yAxis.Minimum = Math.Max(0, minTime * 0.8);
                    yAxis.Maximum = maxTime * 1.2;
                }

                _plotModel.InvalidatePlot(true);
            }
            
            // Highlight selected point
            HighlightSelectedPoint();
        }

        private void HighlightSelectedPoint()
        {
            var lineSeries = _plotModel.Series[0] as LineSeries;
            if (lineSeries == null) return;

            int selectedIndex = TimesListBox.SelectedIndex;
            
            // Remove all highlight series
            while (_plotModel.Series.Count > 1)
            {
                _plotModel.Series.RemoveAt(_plotModel.Series.Count - 1);
            }

            // If a point is selected, add a scatter series for highlighting
            if (selectedIndex >= 0 && selectedIndex < lineSeries.Points.Count)
            {
                // Add highlight marker at selected point
                var highlightSeries = new ScatterSeries
                {
                    MarkerType = MarkerType.Circle,
                    MarkerSize = 10,
                    MarkerFill = OxyColor.FromRgb(255, 152, 0), // Orange highlight
                    Title = "Selected"
                };
                
                var point = lineSeries.Points[selectedIndex];
                highlightSeries.Points.Add(new ScatterPoint(point.X, point.Y));
                _plotModel.Series.Add(highlightSeries);
            }
            
            _plotModel.InvalidatePlot(true);
        }

        private void TrendPlot_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SelectPointAtPosition(e.GetPosition(TrendPlot));
            e.Handled = true; // Prevent further processing
        }

        private void TrendPlot_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Completely suppress tooltip and tracking
            e.Handled = true;
        }

        private void SelectPointAtPosition(System.Windows.Point position)
        {
            var lineSeries = _plotModel.Series[0] as LineSeries;
            if (lineSeries == null) return;

            var xAxis = _plotModel.Axes[0];
            var yAxis = _plotModel.Axes[1];

            // Find nearest point using screen coordinates for better clickability
            double minDistance = double.MaxValue;
            int nearestIndex = -1;
            const double clickThreshold = 50; // pixels - very generous for easy clicking
            
            for (int i = 0; i < lineSeries.Points.Count; i++)
            {
                // Convert data point to screen coordinates
                double screenX = xAxis.Transform(lineSeries.Points[i].X);
                double screenY = yAxis.Transform(lineSeries.Points[i].Y);
                
                double dx = screenX - position.X;
                double dy = screenY - position.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                
                if (distance < minDistance && distance < clickThreshold)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            if (nearestIndex >= 0)
            {
                TimesListBox.SelectedIndex = nearestIndex;
            }
        }
        private void InitializeDispatcherTimer()
        {
            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(10);
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            double elapsedSeconds = _timerService.GetElapsedSeconds();
            string formatted = _statisticsService.FormatTime(elapsedSeconds);
            TimerDisplay.Text = formatted;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopTimer();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void StartTimer()
        {
            if (!_timerService.IsRunning)
            {
                _timerService.Start();
                _dispatcherTimer.Start();
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                StatusText.Text = "Timer running...";
            }
        }

        private void StopTimer()
        {
            if (_timerService.IsRunning)
            {
                double elapsedSeconds = _timerService.Stop();
                _dispatcherTimer.Stop();

                string formattedTime = _statisticsService.FormatTime(elapsedSeconds);
                TimesListBox.Items.Add(formattedTime);

                UpdateStatistics();
                UpdatePlot();
                
                // Reset the timer after recording
                _timerService.Reset();
                TimerDisplay.Text = "00:00.00";
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                StatusText.Text = "Time recorded!";
            }
        }

        private void ResetTimer()
        {
            _timerService.Reset();
            _timerService.ClearHistory();
            _dispatcherTimer.Stop();
            TimerDisplay.Text = "00:00.00";
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StatusText.Text = "Press SPACE to start";
            
            // Clear all historical data
            TimesListBox.Items.Clear();
            BestTimeText.Text = "--:--";
            WorstTimeText.Text = "--:--";
            AverageTimeText.Text = "--:--";
            
            // Clear the plot
            var lineSeries = _plotModel.Series[0] as OxyPlot.Series.LineSeries;
            if (lineSeries != null)
            {
                lineSeries.Points.Clear();
            }
            _plotModel.Axes[0].Maximum = 10;
            _plotModel.Axes[1].Maximum = 10;
            _plotModel.InvalidatePlot(true);
        }

        private void UpdateStatistics()
        {
            var times = _timerService.GetRecordedTimes();
            if (times.Count > 0)
            {
                double best = _statisticsService.GetBestTime(times);
                double worst = _statisticsService.GetWorstTime(times);
                double average = _statisticsService.GetAverageTime(times);

                BestTimeText.Text = _statisticsService.FormatTime(best);
                WorstTimeText.Text = _statisticsService.FormatTime(worst);
                AverageTimeText.Text = _statisticsService.FormatTime(average);
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                
                if (!_spacePressed)
                {
                    _spacePressed = true;
                    _holdStopwatch.Restart();
                    StatusText.Text = "Hold for 0.5 seconds...";
                }
            }
            else if (e.Key == Key.R)
            {
                e.Handled = true;
                ResetTimer();
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                _holdStopwatch.Stop();
                
                if (_spacePressed)
                {
                    _spacePressed = false;
                    long elapsedMs = _holdStopwatch.ElapsedMilliseconds;
                    
                    // If timer is running, stop it immediately (no hold required)
                    if (_timerService.IsRunning)
                    {
                        StopTimer();
                    }
                    // If timer is not running, require 0.5 second hold to start
                    else if (elapsedMs >= 500)
                    {
                        StartTimer();
                        StatusText.Text = "Timer started!";
                    }
                    else
                    {
                        StatusText.Text = $"Hold longer... ({elapsedMs}ms)";
                    }
                }
            }
        }

        private void TimesListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                
                if (TimesListBox.SelectedIndex >= 0)
                {
                    int selectedIndex = TimesListBox.SelectedIndex;
                    
                    // Remove from ListBox
                    TimesListBox.Items.RemoveAt(selectedIndex);
                    
                    // Remove from service
                    _timerService.RemoveTime(selectedIndex);
                    
                    // Update UI
                    UpdateStatistics();
                    UpdatePlot();
                    StatusText.Text = "Time deleted!";
                }
            }
        }

        private void TimesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            HighlightSelectedPoint();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (TimesListBox.SelectedIndex >= 0)
            {
                int selectedIndex = TimesListBox.SelectedIndex;
                
                // Remove from ListBox
                TimesListBox.Items.RemoveAt(selectedIndex);
                
                // Remove from service
                _timerService.RemoveTime(selectedIndex);
                
                // Update UI
                UpdateStatistics();
                UpdatePlot();
                StatusText.Text = "Time deleted!";
            }
            
            // Return focus to MainWindow for keyboard input
            this.Focus();
        }
    }
}
