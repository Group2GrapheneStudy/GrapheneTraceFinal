using GrapheneTrace.Models;

namespace GrapheneTrace.Services
{
    public class PressureAnalysisService : IPressureAnalysisService
    {
        // Returns the highest pressure value in the matrix
        public decimal CalculatePeakPressure(int[,] matrix)
        {
            int max = 0;
            foreach (var v in matrix)
            {
                if (v > max) max = v;
            }
            return max;
        }

        // Returns the average pressure across all sensor values
        public decimal CalculateAveragePressure(int[,] matrix)
        {
            long total = 0;
            int count = 0;

            foreach (var v in matrix)
            {
                total += v;
                count++;
            }

            return count == 0 ? 0 : (decimal)total / count;
        }

        // Returns the percent of cells above a given activation threshold
        public decimal CalculateContactAreaPercent(int[,] matrix, int threshold = 10)
        {
            int active = 0;
            int total = matrix.Length;

            foreach (var v in matrix)
            {
                if (v >= threshold) active++;
            }

            return total == 0 ? 0 : (decimal)active / total * 100m;
        }

        // Returns a simple risk score based on peak pressure and contact area
        public decimal CalculateRiskScore(decimal peak, decimal contactArea)
        {
            // 0–100 simple score: 60% from peak, 40% from contact area
            decimal score = (peak / 255m) * 60m + (contactArea / 100m) * 40m;
            return Math.Round(score, 2);
        }
    }
}
