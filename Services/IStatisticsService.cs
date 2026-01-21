using System;
using System.Collections.Generic;

namespace SpeedCubeTimer.Services
{
    /// <summary>
    /// Interface for calculating statistics from recorded times
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Gets the best (minimum) time
        /// </summary>
        /// <param name="times">List of times in seconds</param>
        /// <returns>Best time in seconds, or 0 if no times recorded</returns>
        double GetBestTime(IEnumerable<double> times);

        /// <summary>
        /// Gets the worst (maximum) time
        /// </summary>
        /// <param name="times">List of times in seconds</param>
        /// <returns>Worst time in seconds, or 0 if no times recorded</returns>
        double GetWorstTime(IEnumerable<double> times);

        /// <summary>
        /// Gets the average time
        /// </summary>
        /// <param name="times">List of times in seconds</param>
        /// <returns>Average time in seconds, or 0 if no times recorded</returns>
        double GetAverageTime(IEnumerable<double> times);

        /// <summary>
        /// Formats time from seconds to MM:SS.MS format
        /// </summary>
        /// <param name="seconds">Time in seconds</param>
        /// <returns>Formatted time string</returns>
        string FormatTime(double seconds);
    }
}
