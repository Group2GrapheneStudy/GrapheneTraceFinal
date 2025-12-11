using GrapheneTrace.Services;
using Microsoft.AspNetCore.Mvc;

namespace GrapheneTrace.Controllers
{
    [Route("api/heatmap")]
    [ApiController]
    public class HeatmapApiController : ControllerBase
    {
        // Service responsible for retrieving heatmap frame data
        private readonly IHeatmapService _heatmapService;

        public HeatmapApiController(IHeatmapService heatmapService)
        {
            _heatmapService = heatmapService;
        }

        // GET api/heatmap/frame/{frameId}
        // Returns a heatmap matrix and calculate metrics for a single frame
        [HttpGet("frame/{frameId}")]
        public async Task<IActionResult> GetHeatmapForFrame(int frameId)
        {
            // Retrieve the heatmap frame from the service
            var result = await _heatmapService.GetHeatmapForFrameAsync(frameId);

            // Return 404 if frame doesn't exist
            if (result == null)
                return NotFound();

            // Prepare a array for JSON
            var rows = result.Values.GetLength(0);
            var cols = result.Values.GetLength(1);
            var values = new int[rows][];

            // Copy 2D array into jagged array structure
            for (int r = 0; r < rows; r++)
            {
                values[r] = new int[cols];
                for (int c = 0; c < cols; c++)
                {
                    values[r][c] = result.Values[r, c];
                }
            }

            // Return structured response containing metadata + matrix
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
