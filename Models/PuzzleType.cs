namespace SpeedCubeTimer.Models
{
    /// <summary>
    /// Represents a puzzle type with its configuration
    /// </summary>
    public class PuzzleType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int Layers { get; set; }
        public bool IsOfficial { get; set; }

        public PuzzleType(string name, string shortName, int layers, bool isOfficial = true)
        {
            Name = name;
            ShortName = shortName;
            Layers = layers;
            IsOfficial = isOfficial;
        }
    }
}
