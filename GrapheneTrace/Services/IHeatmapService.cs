using GrapheneTrace.Models;

namespace GrapheneTrace.Services
{
    public interface IHeatmapService
    {
        Task<HeatmapResult?> GetLatestHeatmapAsync(int patientId);
        Task<HeatmapResult?> GetHeatmapForFrameAsync(int frameId);
    }
}
