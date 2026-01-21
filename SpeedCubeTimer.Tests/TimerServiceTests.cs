using System;
using System.Threading;
using Xunit;
using SpeedCubeTimer.Services;

namespace SpeedCubeTimer.Tests
{
    /// <summary>
    /// Unit tests for TimerService
    /// </summary>
    public class TimerServiceTests
    {
        private readonly ITimerService _timerService;

        public TimerServiceTests()
        {
            _timerService = new TimerService();
        }

        [Fact]
        public void TimerService_InitialState_IsNotRunning()
        {
            // Arrange & Act & Assert
            Assert.False(_timerService.IsRunning);
        }

        [Fact]
        public void TimerService_Start_StartsTimer()
        {
            // Arrange
            // Act
            _timerService.Start();

            // Assert
            Assert.True(_timerService.IsRunning);
        }

        [Fact]
        public void TimerService_Stop_StopsTimer()
        {
            // Arrange
            _timerService.Start();

            // Act
            _timerService.Stop();

            // Assert
            Assert.False(_timerService.IsRunning);
        }

        [Fact]
        public void TimerService_Stop_RecordsTime()
        {
            // Arrange
            _timerService.Start();
            System.Threading.Thread.Sleep(100);

            // Act
            double elapsedTime = _timerService.Stop();

            // Assert
            Assert.True(elapsedTime >= 0.1, "Elapsed time should be at least 0.1 seconds");
            Assert.Single(_timerService.GetRecordedTimes());
        }

        [Fact]
        public void TimerService_Reset_ClearsTimer()
        {
            // Arrange
            _timerService.Start();
            System.Threading.Thread.Sleep(50);

            // Act
            _timerService.Reset();

            // Assert
            Assert.Equal(0, _timerService.GetElapsedSeconds(), 2);
            Assert.False(_timerService.IsRunning);
        }

        [Fact]
        public void TimerService_GetElapsedSeconds_ReturnsPositiveValue()
        {
            // Arrange
            _timerService.Start();
            System.Threading.Thread.Sleep(50);

            // Act
            double elapsed = _timerService.GetElapsedSeconds();

            // Assert
            Assert.True(elapsed >= 0.05, "Elapsed should be at least 50ms");
        }

        [Fact]
        public void TimerService_MultipleSolves_RecordsAllTimes()
        {
            // Arrange
            // Act
            for (int i = 0; i < 3; i++)
            {
                _timerService.Start();
                System.Threading.Thread.Sleep(50 + i * 20);
                _timerService.Stop();
            }

            // Assert
            Assert.Equal(3, _timerService.GetRecordedTimes().Count);
        }

        [Fact]
        public void TimerService_ClearHistory_RemovesRecordedTimes()
        {
            // Arrange
            _timerService.Start();
            _timerService.Stop();
            Assert.Single(_timerService.GetRecordedTimes());

            // Act
            _timerService.ClearHistory();

            // Assert
            Assert.Empty(_timerService.GetRecordedTimes());
        }

        [Fact]
        public void TimerService_NoDoubleStart_DoesNotRestartTimer()
        {
            // Arrange
            _timerService.Start();
            System.Threading.Thread.Sleep(50);
            double firstElapsed = _timerService.GetElapsedSeconds();

            // Act
            _timerService.Start();
            System.Threading.Thread.Sleep(50);
            double secondElapsed = _timerService.GetElapsedSeconds();

            // Assert
            // Both should be measuring from same start
            Assert.True(secondElapsed > firstElapsed);
            Assert.True(_timerService.IsRunning);
        }

        [Fact]
        public void TimerService_RemoveTime_RemovesSpecificIndex()
        {
            // Arrange - Record 3 times
            _timerService.Start();
            _timerService.Stop();
            _timerService.Start();
            _timerService.Stop();
            _timerService.Start();
            _timerService.Stop();
            Assert.Equal(3, _timerService.GetRecordedTimes().Count);

            // Act - Remove middle time (index 1)
            _timerService.RemoveTime(1);

            // Assert
            Assert.Equal(2, _timerService.GetRecordedTimes().Count);
        }

        [Fact]
        public void TimerService_StopRecordsTimeBeforeReturning()
        {
            // Arrange
            _timerService.Start();
            System.Threading.Thread.Sleep(100);

            // Act
            double stoppedTime = _timerService.Stop();
            var recordedTimes = _timerService.GetRecordedTimes();

            // Assert
            Assert.NotEmpty(recordedTimes);
            Assert.Equal(stoppedTime, recordedTimes[0], 2);
        }

        [Fact]
        public void TimerService_GetRecordedTimes_ReturnsListInOrder()
        {
            // Arrange & Act
            _timerService.Start();
            System.Threading.Thread.Sleep(100);
            _timerService.Stop();

            _timerService.Start();
            System.Threading.Thread.Sleep(50);
            _timerService.Stop();

            _timerService.Start();
            System.Threading.Thread.Sleep(100);
            _timerService.Stop();

            // Assert
            var times = _timerService.GetRecordedTimes();
            Assert.Equal(3, times.Count);
            Assert.True(times[0] > 0, "First time should be recorded");
            Assert.True(times[1] > 0, "Second time should be recorded");
            Assert.True(times[2] > 0, "Third time should be recorded");
        }
    }
}
