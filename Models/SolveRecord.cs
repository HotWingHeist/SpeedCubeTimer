using System;

namespace SpeedCubeTimer.Models
{
    /// <summary>
    /// Represents a solve time with optional penalty
    /// </summary>
    public enum PenaltyType
    {
        None,
        Plus2,
        DNF
    }

    public class SolveRecord
    {
        public double ElapsedSeconds { get; set; }
        public PenaltyType Penalty { get; set; }
        public DateTime RecordedAt { get; set; }

        public SolveRecord(double elapsedSeconds, PenaltyType penalty = PenaltyType.None)
        {
            ElapsedSeconds = elapsedSeconds;
            Penalty = penalty;
            RecordedAt = DateTime.Now;
        }

        public double GetEffectiveTime()
        {
            return Penalty switch
            {
                PenaltyType.None => ElapsedSeconds,
                PenaltyType.Plus2 => ElapsedSeconds + 2.0,
                PenaltyType.DNF => double.PositiveInfinity,
                _ => ElapsedSeconds
            };
        }

        public string GetDisplayTime()
        {
            if (Penalty == PenaltyType.DNF)
                return "DNF";
            
            string baseTime = FormatTime(ElapsedSeconds);
            if (Penalty == PenaltyType.Plus2)
                return $"{baseTime} +2";
            
            return baseTime;
        }

        private string FormatTime(double seconds)
        {
            int minutes = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            int ms = (int)((seconds % 1) * 100);
            return $"{minutes}:{secs:D2}.{ms:D2}";
        }
    }
}
