using GrapheneTrace.Data;
using GrapheneTrace.Models;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Services
{
    public class HeatmapService : IHeatmapService
    {
        private readonly AppDbContext _context;
        private readonly IPressureAnalysisService _analysis;

        public HeatmapService(AppDbContext context, IPressureAnalysisService analysis)
        {
            _context = context;
            _analysis = analysis;
        }

        public async Task<HeatmapResult?> GetLatestHeatmapAsync(int patientId)
        {
            var latestFile = await _context.DataFiles
                .Where(df => df.PatientId == patientId)
                .OrderByDescending(df => df.UploadedAt)
                .FirstOrDefaultAsync();

            if (latestFile == null) return null;

            var latestFrame = await _context.PressureFrames
                .Where(f => f.DataFileId == latestFile.DataFileId)
                .OrderByDescending(f => f.CapturedAtUtc)
                .FirstOrDefaultAsync();

            if (latestFrame == null) return null;

            return await BuildHeatmapFromFrameAsync(latestFrame);
        }

        public async Task<HeatmapResult?> GetHeatmapForFrameAsync(int frameId)
        {
            var frame = await _context.PressureFrames
                .Include(f => f.DataFile)
                .FirstOrDefaultAsync(f => f.FrameId == frameId);

            if (frame == null) return null;

            return await BuildHeatmapFromFrameAsync(frame);
        }

        private async Task<HeatmapResult> BuildHeatmapFromFrameAsync(PressureFrame frame)
        {
            string path = frame.DataFile.FilePath;

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"CSV file not found: {path}");
            }

            int[,] matrix = await LoadMatrixForFrameAsync(path, frame.FrameIndex);

            var peak = _analysis.CalculatePeakPressure(matrix);
            var avg = _analysis.CalculateAveragePressure(matrix);
            var contact = _analysis.CalculateContactAreaPercent(matrix);
            var risk = _analysis.CalculateRiskScore(peak, contact);

            return new HeatmapResult
            {
                Values = matrix,
                Timestamp = frame.CapturedAtUtc,
                PeakPressure = peak,
                AveragePressure = avg,
                ContactAreaPercent = contact,
                RiskScore = risk
            };
        }

        // Reads only the 32x32 block for a given frameIndex
        private async Task<int[,]> LoadMatrixForFrameAsync(string filePath, int frameIndex)
        {
            var lines = await File.ReadAllLinesAsync(filePath);

            int rowsPerFrame = 32;
            int cols = 32;

            int startRow = frameIndex * rowsPerFrame;

            if (lines.Length < startRow + rowsPerFrame)
                throw new InvalidOperationException("CSV does not contain enough rows for this frame.");

            int[,] matrix = new int[rowsPerFrame, cols];

            for (int r = 0; r < rowsPerFrame; r++)
            {
                var cells = lines[startRow + r]
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);

                for (int c = 0; c < cols; c++)
                {
                    matrix[r, c] = int.Parse(cells[c]);
                }
            }

            return matrix;
        }
    }
}
