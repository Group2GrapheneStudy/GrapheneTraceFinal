using GrapheneTrace.Models;

namespace GrapheneTrace.Services
{
    public interface IPressureAnalysisService
    {
        decimal CalculatePeakPressure(int[,] matrix);
        decimal CalculateAveragePressure(int[,] matrix);
        decimal CalculateContactAreaPercent(int[,] matrix, int threshold = 10);
        decimal CalculateRiskScore(decimal peak, decimal contactArea);
    }
}
