using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedCubeTimer.Services
{
    /// <summary>
    /// Implementation of statistics calculations
    /// </summary>
    public class StatisticsService : IStatisticsService
    {
        public double GetBestTime(IEnumerable<double> times)
        {
            var timeList = times?.ToList() ?? new List<double>();
            return timeList.Count > 0 ? timeList.Min() : 0;
        }

        public double GetWorstTime(IEnumerable<double> times)
        {
            var timeList = times?.ToList() ?? new List<double>();
            return timeList.Count > 0 ? timeList.Max() : 0;
        }

        public double GetAverageTime(IEnumerable<double> times)
        {
            var timeList = times?.ToList() ?? new List<double>();
            return timeList.Count > 0 ? timeList.Average() : 0;
        }

        public string FormatTime(double seconds)
        {
            int minutes = (int)(seconds / 60);
            double secs = seconds % 60;
            return $"{minutes:D2}:{secs:F2}";
        }
    }
}
