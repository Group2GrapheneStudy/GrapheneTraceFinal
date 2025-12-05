using GrapheneTrace.Services;
using Microsoft.AspNetCore.Mvc;

namespace GrapheneTrace.Controllers
{
    [Route("api/heatmap")]
    [ApiController]
    public class HeatmapApiController : ControllerBase
    {
        private readonly IHeatmapService _heatmapService;

        public HeatmapApiController(IHeatmapService heatmapService)
        {
            _heatmapService = heatmapService;
        }

        [HttpGet("frame/{frameId}")]
        public async Task<IActionResult> GetHeatmapForFrame(int frameId)
        {
            var result = await _heatmapService.GetHeatmapForFrameAsync(frameId);
            if (result == null)
                return NotFound();

            var rows = result.Values.GetLength(0);
            var cols = result.Values.GetLength(1);
            var values = new int[rows][];

            for (int r = 0; r < rows; r++)
            {
                values[r] = new int[cols];
                for (int c = 0; c < cols; c++)
                    values[r][c] = result.Values[r, c];
            }

            return Ok(new
            {
                frameId,
                timestamp = result.Timestamp,
                peakPressure = result.PeakPressure,
                averagePressure = result.AveragePressure,
                contactAreaPercent = result.ContactAreaPercent,
                riskScore = result.RiskScore,
                values
            });
        }
    }
}
