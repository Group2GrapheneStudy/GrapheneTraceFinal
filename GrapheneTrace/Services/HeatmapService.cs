using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

        /// <summary>
        /// Returns the latest heatmap for a given patient based on the most recent
        /// DataFile + PressureFrame.
        /// </summary>
        public async Task<HeatmapResult?> GetLatestHeatmapAsync(int patientId)
        {
            var latestFile = await _context.DataFiles
                .Where(df => df.PatientId == patientId)
                .OrderByDescending(df => df.UploadedAt)
                .FirstOrDefaultAsync();

            if (latestFile == null)
                return null;

            var latestFrame = await _context.PressureFrames
                .Include(f => f.DataFile)          // ensure DataFile is loaded
                .Where(f => f.DataFileId == latestFile.DataFileId)
                .OrderByDescending(f => f.CapturedAtUtc)
                .FirstOrDefaultAsync();

            if (latestFrame == null)
                return null;

            // ✅ Alerts allowed here (if you still want them when asking for "latest")
            return await BuildHeatmapFromFrameAsync(latestFrame, generateAlert: true);
        }

        /// <summary>
        /// Returns a heatmap for a specific frame.
        /// </summary>
        public async Task<HeatmapResult?> GetHeatmapForFrameAsync(int frameId)
        {
            var frame = await _context.PressureFrames
                .Include(f => f.DataFile)
                .FirstOrDefaultAsync(f => f.FrameId == frameId);

            if (frame == null)
                return null;

            // ✅ NO alerts when called by the slider
            return await BuildHeatmapFromFrameAsync(frame, generateAlert: false);
        }

        /// <summary>
        /// Core worker: load the 32x32 matrix for a frame, compute metrics,
        /// optionally raise an alert, and return HeatmapResult.
        /// </summary>
        private async Task<HeatmapResult> BuildHeatmapFromFrameAsync(PressureFrame frame, bool generateAlert)
        {
            if (frame.DataFile == null)
                throw new InvalidOperationException("PressureFrame.DataFile must be included.");

            var path = frame.DataFile.FilePath;

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new FileNotFoundException($"CSV file not found: {path}");

            int[,] matrix = await LoadMatrixForFrameAsync(path, frame.FrameIndex);

            var peak = _analysis.CalculatePeakPressure(matrix);
            var avg = _analysis.CalculateAveragePressure(matrix);
            var contact = _analysis.CalculateContactAreaPercent(matrix);
            var risk = _analysis.CalculateRiskScore(peak, contact);

            // ============================
            // AUTO ALERT GENERATION
            // ============================
            const int highPressureThreshold = 180;

            if (generateAlert && peak > highPressureThreshold)
            {
                var alert = new Alert
                {
                    PatientId = frame.DataFile.PatientId,
                    FrameId = frame.FrameId,
                    AlertType = "High Pressure Detected",
                    Severity = peak > 220 ? "Critical" : "High",
                    Message = $"High pressure region detected (Peak={peak}).",
                    RaisedByUserId = frame.DataFile.UploadedByUserId,
                    TriggeredAt = DateTime.UtcNow,
                    Status = "Unresolved"
                };

                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();
            }

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

        /// <summary>
        /// Reads only the 32x32 block for a given frameIndex from the CSV.
        /// </summary>
        private async Task<int[,]> LoadMatrixForFrameAsync(string filePath, int frameIndex)
        {
            var lines = await File.ReadAllLinesAsync(filePath);

            const int rowsPerFrame = 32;
            const int cols = 32;

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
