using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SpeedCubeTimer.Services
{
    /// <summary>
    /// Implementation of the timer service using Stopwatch
    /// </summary>
    public class TimerService : ITimerService
    {
        private readonly Stopwatch _stopwatch;
        private readonly List<double> _recordedTimes;
        private bool _isRunning;

        public TimerService()
        {
            _stopwatch = new Stopwatch();
            _recordedTimes = new List<double>();
            _isRunning = false;
        }

        public bool IsRunning => _isRunning;

        public void Start()
        {
            if (!_isRunning)
            {
                _stopwatch.Start();
                _isRunning = true;
            }
        }

        public double Stop()
        {
            if (_isRunning)
            {
                _stopwatch.Stop();
                _isRunning = false;
                double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
                _recordedTimes.Add(elapsedSeconds);
                return elapsedSeconds;
            }
            return 0;
        }

        public void Reset()
        {
            _stopwatch.Reset();
            _isRunning = false;
        }

        public double GetElapsedSeconds()
        {
            return _stopwatch.Elapsed.TotalSeconds;
        }

        public IReadOnlyList<double> GetRecordedTimes()
        {
            return _recordedTimes.AsReadOnly();
        }

        public void ClearHistory()
        {
            _recordedTimes.Clear();
        }

        public void RemoveTime(int index)
        {
            if (index >= 0 && index < _recordedTimes.Count)
            {
                _recordedTimes.RemoveAt(index);
            }
        }
    }
}
