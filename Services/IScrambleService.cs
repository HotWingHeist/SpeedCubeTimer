using System;
using System.Collections.Generic;
using SpeedCubeTimer.Models;

namespace SpeedCubeTimer.Services
{
    /// <summary>
    /// Interface for generating scrambles based on puzzle type
    /// </summary>
    public interface IScrambleService
    {
        string GenerateScramble(PuzzleType puzzle);
    }

    /// <summary>
    /// Service for generating valid cube scrambles
    /// </summary>
    public class ScrambleService : IScrambleService
    {
        private static readonly string[] Moves = { "U", "D", "L", "R", "F", "B", "M", "E", "S", "x", "y", "z" };
        private static readonly string[] Modifiers = { "", "'", "2" };
        private readonly Random _random;

        public ScrambleService()
        {
            _random = new Random();
        }

        public string GenerateScramble(PuzzleType puzzle)
        {
            int moveCount = GetMoveCount(puzzle.Layers);
            var scrambleMoves = new List<string>();
            char lastAxis = ' ';

            for (int i = 0; i < moveCount; i++)
            {
                string move;
                do
                {
                    move = GenerateRandomMove(puzzle.Layers);
                } while (move.Length > 0 && move[0] == lastAxis); // Avoid same axis twice in a row

                if (move.Length > 0)
                {
                    lastAxis = move[0];
                }
                scrambleMoves.Add(move);
            }

            return string.Join(" ", scrambleMoves);
        }

        private string GenerateRandomMove(int layers)
        {
            var validMoves = GetValidMovesForPuzzle(layers);
            string move = validMoves[_random.Next(validMoves.Count)];
            string modifier = Modifiers[_random.Next(Modifiers.Length)];
            return move + modifier;
        }

        private List<string> GetValidMovesForPuzzle(int layers)
        {
            var moves = new List<string> { "U", "D", "L", "R", "F", "B" };

            if (layers >= 4)
            {
                moves.AddRange(new[] { "M", "E", "S", "Uw", "Dw", "Lw", "Rw", "Fw", "Bw" });
            }

            if (layers >= 5)
            {
                moves.AddRange(new[] { "2U", "2D", "2L", "2R", "2F", "2B" });
            }

            return moves;
        }

        private int GetMoveCount(int layers)
        {
            return layers switch
            {
                2 => 9,
                3 => 25,
                4 => 40,
                5 => 60,
                6 => 80,
                7 => 100,
                _ => 25
            };
        }
    }
}
