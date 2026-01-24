using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using SpeedCubeTimer.Services;
using SpeedCubeTimer.Models;

namespace SpeedCubeTimer
{
    public partial class MainWindow : Window
    {
        private readonly ITimerService _timerService;
        private readonly IStatisticsService _statisticsService;
        private readonly IScrambleService _scrambleService;
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        private System.Windows.Threading.DispatcherTimer _inspectionTimer;
        private PlotModel _plotModel;
        private bool _isRunning = false;
        private bool _spacePressed = false;
        private Stopwatch _holdStopwatch;
        private PuzzleType _selectedPuzzle;
        private PenaltyType _currentPenalty = PenaltyType.None;
        private int _inspectionTimeRemaining = 15;
        private List<SolveRecord> _solveRecords = new List<SolveRecord>(); // Track solves with penalties

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize services
            _timerService = new TimerService();
            _statisticsService = new StatisticsService();
            _scrambleService = new ScrambleService();
            _holdStopwatch = new Stopwatch();
            
            // Initialize puzzle types with some standard puzzles
            var puzzles = new List<PuzzleType>
            {
                new PuzzleType("2x2x2", "2x2", 2, true),
                new PuzzleType("3x3x3", "3x3", 3, true),
                new PuzzleType("4x4x4", "4x4", 4, true),
                new PuzzleType("5x5x5", "5x5", 5, true),
                new PuzzleType("6x6x6", "6x6", 6, true),
                new PuzzleType("7x7x7", "7x7", 7, true)
            };
            
            PuzzleSelection.ItemsSource = puzzles;
            PuzzleSelection.DisplayMemberPath = "Name";
            _selectedPuzzle = puzzles.First(p => p.ShortName == "3x3");
            PuzzleSelection.SelectedItem = _selectedPuzzle;
            GenerateNewScramble();
            
            InitializeDispatcherTimer();
            InitializeInspectionTimer();
            InitializePlot();
            this.PreviewKeyDown += MainWindow_KeyDown;
            this.PreviewKeyUp += MainWindow_KeyUp;
            UpdateHistoryUI();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Keep keyboard shortcuts responsive without extra clicks
            this.Focus();
            Keyboard.Focus(this);
        }

        private void InitializePlot()
        {
            _plotModel = new PlotModel 
            { 
                Title = "Solve Times Over Session",
                Background = OxyColor.FromRgb(255, 255, 255),
                PlotAreaBackground = OxyColor.FromRgb(250, 250, 250),
                TextColor = OxyColor.FromRgb(33, 33, 33)
            };

            // Add axes
            _plotModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Bottom,
                Title = "Solve #",
                Minimum = 0,
                TextColor = OxyColor.FromRgb(33, 33, 33),
                TicklineColor = OxyColor.FromRgb(200, 200, 200)
            });
            
            _plotModel.Axes.Add(new LinearAxis 
            { 
                Position = AxisPosition.Left,
                Title = "Time (seconds)",
                Minimum = 0,
                TextColor = OxyColor.FromRgb(33, 33, 33),
                TicklineColor = OxyColor.FromRgb(200, 200, 200)
            });

            // Add line series
            var lineSeries = new LineSeries
            {
                Title = "Solve Times",
                Color = OxyColor.FromRgb(2, 158, 115),
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 12,
                MarkerFill = OxyColor.FromRgb(2, 158, 115),
                TrackerFormatString = "",
                CanTrackerInterpolatePoints = false
            };
            _plotModel.Series.Add(lineSeries);

            TrendPlot.Model = _plotModel;
        }

        private void UpdatePlot()
        {
            if (_plotModel == null || _plotModel.Series == null || _plotModel.Series.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("UpdatePlot: Plot model not initialized");
                return;
            }
                
            System.Diagnostics.Debug.WriteLine($"UpdatePlot called: {_solveRecords?.Count ?? 0} solves");
            
            var lineSeries = _plotModel.Series[0] as LineSeries;
            if (lineSeries == null)
            {
                System.Diagnostics.Debug.WriteLine("UpdatePlot: Line series is null");
                return;
            }
            
            // Clear existing points
            lineSeries.Points.Clear();
            
            // Remove all series except the line series
            while (_plotModel.Series.Count > 1)
            {
                _plotModel.Series.RemoveAt(_plotModel.Series.Count - 1);
            }
            
            if (_solveRecords == null || _solveRecords.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("UpdatePlot: No solve records");
                _plotModel.Axes[0].Maximum = 10;
                _plotModel.Axes[0].Minimum = 0;
                _plotModel.Axes[1].Maximum = 10;
                _plotModel.Axes[1].Minimum = 0;
                _plotModel.InvalidatePlot(true);
                return;
            }
            
            // Add all points
            for (int i = 0; i < _solveRecords.Count; i++)
            {
                double effectiveTime = _solveRecords[i].GetEffectiveTime();
                // Only plot valid times (skip DNF which is infinity)
                if (!double.IsInfinity(effectiveTime))
                {
                    lineSeries.Points.Add(new DataPoint(i + 1, effectiveTime));
                    System.Diagnostics.Debug.WriteLine($"  Point {i + 1}: {effectiveTime}s");
                }
            }

            // Update axis limits
            var xAxis = _plotModel.Axes[0];
            var yAxis = _plotModel.Axes[1];
            
            xAxis.Maximum = _solveRecords.Count + 1;
            xAxis.Minimum = 0;
            
            // Get all valid times for y-axis scaling
            var validTimes = _solveRecords
                .Select(r => r.GetEffectiveTime())
                .Where(t => !double.IsInfinity(t))
                .ToList();
            
            if (validTimes.Count > 0)
            {
                double maxTime = validTimes.Max();
                double minTime = validTimes.Min();
                yAxis.Minimum = Math.Max(0, minTime * 0.8);
                yAxis.Maximum = maxTime * 1.2;
                System.Diagnostics.Debug.WriteLine($"  Y-axis: {yAxis.Minimum:F2} to {yAxis.Maximum:F2}");
            }
            else
            {
                yAxis.Maximum = 10;
                yAxis.Minimum = 0;
            }

            _plotModel.InvalidatePlot(true);
            System.Diagnostics.Debug.WriteLine($"UpdatePlot complete: {lineSeries.Points.Count} points added");
            
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
            StopTimerWithPenalty();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTimer();
        }

        private void StartTimer()
        {
            if (!_timerService.IsRunning)
            {
                // Stop inspection timer if it's running
                if (_inspectionTimer.IsEnabled)
                {
                    _inspectionTimer.Stop();
                    InspectionTimerDisplay.Text = "";
                }
                
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
                TimesListBox.ScrollIntoView(TimesListBox.Items[TimesListBox.Items.Count - 1]);

                UpdateStatistics();
                UpdatePlot();
                UpdateHistoryUI();
                
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
            _inspectionTimer.Stop();
            TimerDisplay.Text = "00:00.00";
            InspectionTimerDisplay.Text = "";
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            StatusText.Text = "Press SPACE to start";
            _currentPenalty = PenaltyType.None;
            
            // Clear all historical data
            _solveRecords.Clear();
            TimesListBox.Items.Clear();
            BestTimeText.Text = "--:--";
            WorstTimeText.Text = "--:--";
            AverageTimeText.Text = "--:--";
            UpdateHistoryUI();
            
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
            if (_solveRecords == null || _solveRecords.Count == 0)
            {
                BestTimeText.Text = "--:--";
                WorstTimeText.Text = "--:--";
                AverageTimeText.Text = "--:--";
                return;
            }
            
            // Get effective times (with penalties applied)
            var effectiveTimes = _solveRecords.Select(r => r.GetEffectiveTime()).ToList();
            
            // Filter out DNF (infinity) for best/average calculation
            var validTimes = effectiveTimes.Where(t => !double.IsInfinity(t)).ToList();
            
            double best = validTimes.Count > 0 ? _statisticsService.GetBestTime(validTimes) : 0;
            double worst = effectiveTimes.Count > 0 ? _statisticsService.GetWorstTime(effectiveTimes) : 0;
            double average = validTimes.Count > 0 ? _statisticsService.GetAverageTime(validTimes) : 0;

            BestTimeText.Text = best > 0 ? _statisticsService.FormatTime(best) : "--:--";
            WorstTimeText.Text = double.IsInfinity(worst) ? "DNF" : (worst > 0 ? _statisticsService.FormatTime(worst) : "--:--");
            AverageTimeText.Text = average > 0 ? _statisticsService.FormatTime(average) : "--:--";
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
                        StopTimerWithPenalty();
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
                    UpdateHistoryUI();
                    StatusText.Text = "Time deleted!";
                }
            }
        }

        private void TimesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            HighlightSelectedPoint();
            UpdateHistoryUI();
            
            // Scroll the selected item into view
            if (TimesListBox.SelectedIndex >= 0)
            {
                TimesListBox.ScrollIntoView(TimesListBox.SelectedItem);
            }
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
                
                // Remove from solve records
                if (selectedIndex < _solveRecords.Count)
                {
                    _solveRecords.RemoveAt(selectedIndex);
                }
                
                // Update UI
                UpdateStatistics();
                UpdatePlot();
                UpdateHistoryUI();
                StatusText.Text = "Time deleted!";
            }
            
            // Return focus to MainWindow for keyboard input
            this.Focus();
        }

        // Puzzle Selection Handler
        private void PuzzleSelection_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PuzzleSelection.SelectedItem is PuzzleType puzzle)
            {
                _selectedPuzzle = puzzle;
                GenerateNewScramble();
                StatusText.Text = $"Puzzle changed to {puzzle.Name}";
            }
        }

        // Inspection Timer Handlers
        private void InitializeInspectionTimer()
        {
            _inspectionTimer = new System.Windows.Threading.DispatcherTimer();
            _inspectionTimer.Interval = TimeSpan.FromSeconds(1);
            _inspectionTimer.Tick += InspectionTimer_Tick;
        }

        private void InspectionTimer_Tick(object? sender, EventArgs e)
        {
            _inspectionTimeRemaining--;
            
            if (_inspectionTimeRemaining > 0)
            {
                InspectionTimerDisplay.Text = $"Inspection: {_inspectionTimeRemaining}s";
                if (_inspectionTimeRemaining == 8)
                {
                    InspectionTimerDisplay.Foreground = System.Windows.Media.Brushes.Orange;
                }
            }
            else
            {
                _inspectionTimer.Stop();
                InspectionTimerDisplay.Text = "Ready!";
                InspectionTimerDisplay.Foreground = System.Windows.Media.Brushes.LimeGreen;
                StatusText.Text = "Inspection time finished";
            }
        }

        private void InspectionStartButton_Click(object sender, RoutedEventArgs e)
        {
            _inspectionTimeRemaining = 15;
            InspectionTimerDisplay.Foreground = System.Windows.Media.Brushes.Orange;
            _inspectionTimer.Start();
            StatusText.Text = "Inspection time started";
        }

        // Penalty Handlers - Apply to the LAST completed solve
        private void Plus2Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Plus2 clicked. _solveRecords count: {_solveRecords?.Count ?? -1}, IsRunning: {_timerService.IsRunning}");
            
            if (_timerService.IsRunning)
            {
                StatusText.Text = "Cannot change penalty while timer is running";
                return;
            }
            
            if (_solveRecords == null || _solveRecords.Count == 0)
            {
                StatusText.Text = $"No solves to apply penalty to (Count: {_solveRecords?.Count ?? 0})";
                System.Diagnostics.Debug.WriteLine("No solves in list!");
                return;
            }
            
            // Get the last solve and update its penalty
            var lastSolve = _solveRecords[_solveRecords.Count - 1];
            System.Diagnostics.Debug.WriteLine($"Last solve: {lastSolve.ElapsedSeconds}s, Penalty: {lastSolve.Penalty}");
            var newRecord = new SolveRecord(lastSolve.ElapsedSeconds, PenaltyType.Plus2);
            _solveRecords[_solveRecords.Count - 1] = newRecord;
            System.Diagnostics.Debug.WriteLine($"Updated to +2: {newRecord.GetDisplayTime()}");
            
            // Update display
            TimesListBox.Items[TimesListBox.Items.Count - 1] = newRecord.GetDisplayTime();
            UpdateStatistics();
            UpdatePlot();
                UpdateHistoryUI();
            
            StatusText.Text = $"✓ +2 penalty applied: {newRecord.GetDisplayTime()}";
            ((Button)sender).Background = new SolidColorBrush(Color.FromArgb(255, 222, 143, 5));
            DNFButton.Background = new SolidColorBrush(Color.FromArgb(255, 213, 94, 0));
            ClearPenaltyButton.Background = new SolidColorBrush(Color.FromArgb(255, 1, 115, 178));
        }

        private void DNFButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timerService.IsRunning)
            {
                StatusText.Text = "Cannot change penalty while timer is running";
                return;
            }
            
            if (_solveRecords == null || _solveRecords.Count == 0)
            {
                StatusText.Text = $"No solves to apply penalty to (Count: {_solveRecords?.Count ?? 0})";
                return;
            }
            
            // Get the last solve and update its penalty
            var lastSolve = _solveRecords[_solveRecords.Count - 1];
            var newRecord = new SolveRecord(lastSolve.ElapsedSeconds, PenaltyType.DNF);
            _solveRecords[_solveRecords.Count - 1] = newRecord;
            
            // Update display
            TimesListBox.Items[TimesListBox.Items.Count - 1] = newRecord.GetDisplayTime();
            UpdateStatistics();
            UpdatePlot();
            UpdateHistoryUI();
            
            StatusText.Text = $"✓ DNF penalty applied: {newRecord.GetDisplayTime()}";
            ((Button)sender).Background = new SolidColorBrush(Color.FromArgb(255, 204, 85, 0));
            Plus2Button.Background = new SolidColorBrush(Color.FromArgb(255, 222, 143, 5));
            ClearPenaltyButton.Background = new SolidColorBrush(Color.FromArgb(255, 1, 115, 178));
        }

        private void ClearPenaltyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_timerService.IsRunning)
            {
                StatusText.Text = "Cannot change penalty while timer is running";
                return;
            }
            
            if (_solveRecords == null || _solveRecords.Count == 0)
            {
                StatusText.Text = $"No solves to clear penalty from (Count: {_solveRecords?.Count ?? 0})";
                return;
            }
            
            // Get the last solve and clear its penalty
            var lastSolve = _solveRecords[_solveRecords.Count - 1];
            var newRecord = new SolveRecord(lastSolve.ElapsedSeconds, PenaltyType.None);
            _solveRecords[_solveRecords.Count - 1] = newRecord;
            
            // Update display
            TimesListBox.Items[TimesListBox.Items.Count - 1] = newRecord.GetDisplayTime();
            UpdateStatistics();
            UpdatePlot();
            UpdateHistoryUI();
            
            StatusText.Text = $"✓ Penalty cleared: {newRecord.GetDisplayTime()}";
            ((Button)sender).Background = new SolidColorBrush(Color.FromArgb(255, 1, 115, 178));
            Plus2Button.Background = new SolidColorBrush(Color.FromArgb(255, 222, 143, 5));
            DNFButton.Background = new SolidColorBrush(Color.FromArgb(255, 213, 94, 0));
        }

        // Scramble Generation
        private void GenerateNewScramble()
        {
            if (_selectedPuzzle != null)
            {
                string scramble = _scrambleService.GenerateScramble(_selectedPuzzle);
                ScrambleText.Text = scramble;
            }
        }

        // Override StopTimer to use SolveRecord instead of double
        private void StopTimerWithPenalty()
        {
            if (_timerService.IsRunning)
            {
                double elapsedSeconds = _timerService.GetElapsedSeconds();
                // Always create record with no penalty - penalties are applied after the solve
                SolveRecord record = new SolveRecord(elapsedSeconds, PenaltyType.None);
                
                _timerService.Stop();
                _timerService.Reset();  // Reset the stopwatch to zero
                _dispatcherTimer.Stop();
                StopButton.IsEnabled = false;
                StartButton.IsEnabled = true;
                _isRunning = false;
                
                // Reset timer display to zero
                TimerDisplay.Text = "00:00.00";
                
                // Add to solve records
                _solveRecords.Add(record);
                System.Diagnostics.Debug.WriteLine($"Added solve #{_solveRecords.Count}: {record.GetDisplayTime()}, total records: {_solveRecords.Count}");
                
                // Add to list display
                TimesListBox.Items.Add(record.GetDisplayTime());
                TimesListBox.ScrollIntoView(TimesListBox.Items[TimesListBox.Items.Count - 1]);
                
                // Update statistics and plot
                UpdateStatistics();
                UpdatePlot();
                UpdateHistoryUI();
                
                // Generate new scramble
                GenerateNewScramble();
                
                StatusText.Text = $"Solve #{_solveRecords.Count}: {record.GetDisplayTime()} - Use penalty buttons if needed";
            }
        }

        private void UpdateHistoryUI()
        {
            DeleteButton.IsEnabled = TimesListBox.SelectedIndex >= 0;
            EmptyHistoryText.Visibility = TimesListBox.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
