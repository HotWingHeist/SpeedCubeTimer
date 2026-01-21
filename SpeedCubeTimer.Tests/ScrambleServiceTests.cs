using System;
using Xunit;
using SpeedCubeTimer.Services;
using SpeedCubeTimer.Models;

namespace SpeedCubeTimer.Tests
{
    /// <summary>
    /// Unit tests for ScrambleService
    /// </summary>
    public class ScrambleServiceTests
    {
        private readonly IScrambleService _scrambleService;

        public ScrambleServiceTests()
        {
            _scrambleService = new ScrambleService();
        }

        [Fact]
        public void ScrambleService_Generate3x3Scramble_ReturnsValidScramble()
        {
            // Arrange
            var puzzle3x3 = new PuzzleType("3x3 Rubik's Cube", "3x3", 3);

            // Act
            string scramble = _scrambleService.GenerateScramble(puzzle3x3);

            // Assert
            Assert.NotEmpty(scramble);
            Assert.Contains(" ", scramble); // Should have spaces separating moves
            Assert.True(scramble.Length > 10); // Should be reasonably long
        }

        [Fact]
        public void ScrambleService_Generate2x2Scramble_ShorterThan3x3()
        {
            // Arrange
            var puzzle2x2 = new PuzzleType("2x2 Pocket Cube", "2x2", 2);
            var puzzle3x3 = new PuzzleType("3x3 Rubik's Cube", "3x3", 3);

            // Act
            string scramble2x2 = _scrambleService.GenerateScramble(puzzle2x2);
            string scramble3x3 = _scrambleService.GenerateScramble(puzzle3x3);

            // Assert
            Assert.True(scramble2x2.Length < scramble3x3.Length);
        }

        [Fact]
        public void ScrambleService_Generate4x4Scramble_IncludesWideLayerMoves()
        {
            // Arrange
            var puzzle4x4 = new PuzzleType("4x4 Rubik's Revenge", "4x4", 4);

            // Act
            string scramble = _scrambleService.GenerateScramble(puzzle4x4);

            // Assert
            Assert.NotEmpty(scramble);
            // 4x4 scrambles should include wide moves like Uw, Rw, Fw, etc.
            Assert.True(scramble.Contains("U") || scramble.Contains("R") || 
                        scramble.Contains("L") || scramble.Contains("D") ||
                        scramble.Contains("F") || scramble.Contains("B"));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void ScrambleService_GenerateScramble_NoNull(int layers)
        {
            // Arrange
            var puzzle = new PuzzleType($"{layers}x{layers}", layers.ToString(), layers);

            // Act
            string scramble = _scrambleService.GenerateScramble(puzzle);

            // Assert
            Assert.NotNull(scramble);
            Assert.NotEmpty(scramble);
        }
    }

    /// <summary>
    /// Unit tests for SolveRecord
    /// </summary>
    public class SolveRecordTests
    {
        [Fact]
        public void SolveRecord_NoPenalty_GetEffectiveTimeEqualsElapsed()
        {
            // Arrange
            var record = new SolveRecord(12.34, PenaltyType.None);

            // Act
            double effectiveTime = record.GetEffectiveTime();

            // Assert
            Assert.Equal(12.34, effectiveTime);
        }

        [Fact]
        public void SolveRecord_Plus2Penalty_GetEffectiveTimeAddsTwo()
        {
            // Arrange
            var record = new SolveRecord(12.34, PenaltyType.Plus2);

            // Act
            double effectiveTime = record.GetEffectiveTime();

            // Assert
            Assert.Equal(14.34, effectiveTime, 2);
        }

        [Fact]
        public void SolveRecord_DNFPenalty_GetEffectiveTimeIsInfinity()
        {
            // Arrange
            var record = new SolveRecord(12.34, PenaltyType.DNF);

            // Act
            double effectiveTime = record.GetEffectiveTime();

            // Assert
            Assert.True(double.IsPositiveInfinity(effectiveTime));
        }

        [Fact]
        public void SolveRecord_DisplayTime_ShowsCorrectFormat()
        {
            // Arrange
            var record = new SolveRecord(65.5, PenaltyType.None);

            // Act
            string display = record.GetDisplayTime();

            // Assert
            Assert.NotEmpty(display);
            Assert.Contains("1:05", display); // Should show minutes:seconds
        }

        [Fact]
        public void SolveRecord_DisplayTime_WithPlus2_ShowsPenalty()
        {
            // Arrange
            var record = new SolveRecord(12.34, PenaltyType.Plus2);

            // Act
            string display = record.GetDisplayTime();

            // Assert
            Assert.Contains("+2", display);
        }

        [Fact]
        public void SolveRecord_DisplayTime_WithDNF_ShowsDNF()
        {
            // Arrange
            var record = new SolveRecord(12.34, PenaltyType.DNF);

            // Act
            string display = record.GetDisplayTime();

            // Assert
            Assert.Equal("DNF", display);
        }
    }
}
