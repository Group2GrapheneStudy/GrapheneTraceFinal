namespace GrapheneTrace.Models
{
    public class HeatmapResult
    {
        // 32x32 pressure values
        public int[,] Values { get; set; } = new int[32, 32];

        public DateTime? Timestamp { get; set; }

        public decimal PeakPressure { get; set; }
        public decimal AveragePressure { get; set; }
        public decimal ContactAreaPercent { get; set; }
        public decimal RiskScore { get; set; }
    }
}
