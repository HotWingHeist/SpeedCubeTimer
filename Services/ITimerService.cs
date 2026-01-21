using System.Collections.Generic;

namespace SpeedCubeTimer.Services
{
    /// <summary>
    /// Interface for timer service - separates timer logic from UI concerns
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// Starts the timer
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the timer and records the elapsed time
        /// </summary>
        /// <returns>Elapsed time in seconds</returns>
        double Stop();

        /// <summary>
        /// Resets the timer
        /// </summary>
        void Reset();

        /// <summary>
        /// Gets the current elapsed time in seconds
        /// </summary>
        /// <returns>Elapsed time in seconds</returns>
        double GetElapsedSeconds();

        /// <summary>
        /// Gets all recorded times
        /// </summary>
        /// <returns>List of recorded times in seconds</returns>
        IReadOnlyList<double> GetRecordedTimes();

        /// <summary>
        /// Clears all recorded times
        /// </summary>
        void ClearHistory();

        /// <summary>
        /// Removes a recorded time at the specified index
        /// </summary>
        /// <param name="index">Index of the time to remove</param>
        void RemoveTime(int index);

        /// <summary>
        /// Gets whether the timer is currently running
        /// </summary>
        bool IsRunning { get; }
    }
}
