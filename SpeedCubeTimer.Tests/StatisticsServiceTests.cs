using System;
using System.Collections.Generic;
using Xunit;
using SpeedCubeTimer.Services;

namespace SpeedCubeTimer.Tests
{
    /// <summary>
    /// Unit tests for StatisticsService
    /// </summary>
    public class StatisticsServiceTests
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsServiceTests()
        {
            _statisticsService = new StatisticsService();
        }

        [Fact]
        public void StatisticsService_GetBestTime_WithValidTimes_ReturnsBestTime()
        {
            // Arrange
            var times = new[] { 10.5, 9.2, 11.3, 8.9 };

            // Act
            double best = _statisticsService.GetBestTime(times);

            // Assert
            Assert.Equal(8.9, best);
        }

        [Fact]
        public void StatisticsService_GetBestTime_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var times = new double[] { };

            // Act
            double best = _statisticsService.GetBestTime(times);

            // Assert
            Assert.Equal(0, best);
        }

        [Fact]
        public void StatisticsService_GetBestTime_WithNull_ReturnsZero()
        {
            // Arrange
            IEnumerable<double>? times = null;

            // Act
            double best = _statisticsService.GetBestTime(times!);

            // Assert
            Assert.Equal(0, best);
        }

        [Fact]
        public void StatisticsService_GetWorstTime_WithValidTimes_ReturnsWorstTime()
        {
            // Arrange
            var times = new[] { 10.5, 9.2, 11.3, 8.9 };

            // Act
            double worst = _statisticsService.GetWorstTime(times);

            // Assert
            Assert.Equal(11.3, worst);
        }

        [Fact]
        public void StatisticsService_GetWorstTime_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var times = new double[] { };

            // Act
            double worst = _statisticsService.GetWorstTime(times);

            // Assert
            Assert.Equal(0, worst);
        }

        [Fact]
        public void StatisticsService_GetAverageTime_WithValidTimes_ReturnsAverageTime()
        {
            // Arrange
            var times = new[] { 10.0, 20.0, 30.0 };

            // Act
            double average = _statisticsService.GetAverageTime(times);

            // Assert
            Assert.Equal(20.0, average);
        }

        [Fact]
        public void StatisticsService_GetAverageTime_WithEmptyList_ReturnsZero()
        {
            // Arrange
            var times = new double[] { };

            // Act
            double average = _statisticsService.GetAverageTime(times);

            // Assert
            Assert.Equal(0, average);
        }

        [Theory]
        [InlineData(5.23, "00:05.23")]
        [InlineData(65.5, "01:05.50")]
        [InlineData(125.99, "02:05.99")]
        [InlineData(0.5, "00:00.50")]
        [InlineData(3661.25, "61:01.25")]
        public void StatisticsService_FormatTime_ReturnsCorrectFormat(double seconds, string expected)
        {
            // Arrange
            // Act
            string formatted = _statisticsService.FormatTime(seconds);

            // Assert
            Assert.Equal(expected, formatted);
        }

        [Fact]
        public void StatisticsService_Statistics_WithComplexDataSet()
        {
            // Arrange
            var times = new[] { 12.45, 11.23, 13.67, 10.89, 12.34 };

            // Act
            double best = _statisticsService.GetBestTime(times);
            double worst = _statisticsService.GetWorstTime(times);
            double average = _statisticsService.GetAverageTime(times);

            // Assert
            Assert.Equal(10.89, best);
            Assert.Equal(13.67, worst);
            Assert.True(average > 11 && average < 13, "Average should be between best and worst");
            Assert.Equal(12.116, average, 2);
        }
    }
}
