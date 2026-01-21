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
        [InlineData(5.23, "5.23")]
        [InlineData(65.5, "1:5.50")]
        [InlineData(125.99, "2:5.99")]
        [InlineData(0.5, "0.50")]
        public void StatisticsService_FormatTime_ReturnsCorrectFormat(double seconds, string expected)
        {
            // Arrange
            // Act
            string formatted = _statisticsService.FormatTime(seconds);

            // Assert
            Assert.Contains(expected, formatted);
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

        [Fact]
        public void StatisticsService_BestWorstAverage_AllUpdatedCorrectly()
        {
            // Arrange - Requirement: Statistics should show best, average, worst
            var times = new[] { 15.5, 12.3, 14.8 };

            // Act
            double best = _statisticsService.GetBestTime(times);
            double average = _statisticsService.GetAverageTime(times);
            double worst = _statisticsService.GetWorstTime(times);

            // Assert
            Assert.Equal(12.3, best, 1);  // Best time from story 2
            Assert.Equal(14.2, average, 1);  // Average time from story 2
            Assert.Equal(15.5, worst, 1);  // Worst time from story 2
        }

        [Fact]
        public void StatisticsService_FormatTime_DisplaysCorrectly()
        {
            // Arrange - Requirement: Times should be formatted as MM:SS.MS
            
            // Act & Assert
            Assert.Contains("5.23", _statisticsService.FormatTime(5.23));
            Assert.Contains("1:5.50", _statisticsService.FormatTime(65.5));
            Assert.Contains("0.", _statisticsService.FormatTime(0.0));
        }

        [Fact]
        public void StatisticsService_GetAverageTime_WithSingleTime_ReturnsThatTime()
        {
            // Arrange
            var times = new[] { 10.5 };

            // Act
            double average = _statisticsService.GetAverageTime(times);

            // Assert
            Assert.Equal(10.5, average);
        }

        [Theory]
        [InlineData(new[] { 1.0 }, 1.0)]
        [InlineData(new[] { 1.0, 2.0 }, 1.5)]
        [InlineData(new[] { 1.0, 2.0, 3.0 }, 2.0)]
        [InlineData(new[] { 5.0, 10.0, 15.0, 20.0 }, 12.5)]
        public void StatisticsService_GetAverageTime_WithMultipleValues_CalculatesCorrectly(double[] times, double expected)
        {
            // Arrange & Act
            double average = _statisticsService.GetAverageTime(times);

            // Assert
            Assert.Equal(expected, average, 2);
        }
    }
}
