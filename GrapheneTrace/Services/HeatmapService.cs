using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Services
{
    // Provides heatmap-related data operations and analysis
    public class HeatmapService : IHeatmapService
    {
        // Database context for retrieving frames and data files
        private readonly AppDbContext _context;

        // Service used to perform pressure/heatmap analysis calculations
        private readonly IPressureAnalysisService _analysis;

        public HeatmapService(AppDbContext context, IPressureAnalysisService analysis)
        {
            _context = context;
            _analysis = analysis;
        }

        /// <summary>
        /// Returns the latest heatmap for a given patient based on the most recent
        /// DataFile + PressureFrame
        /// </summary>
        public async Task<HeatmapResult?> GetLatestHeatmapAsync(int patientId)
        {
            // Get the latest uploaded data file for this patient
            var latestFile = await _context.DataFiles
                .Where(df => df.PatientId == patientId)
                .OrderByDescending(df => df.UploadedAt)
                .FirstOrDefaultAsync();

            // If the patient has no uploads, no heatmap can be generated
            if (latestFile == null)
                return null;

            // Get the most recent frame from that file
            var latestFrame = await _context.PressureFrames
                .Include(f => f.DataFile) // ensure DataFile is loaded
                .Where(f => f.DataFileId == latestFile.DataFileId)
                .OrderByDescending(f => f.CapturedAtUtc)
                .FirstOrDefaultAsync();

            // If the file contains no frames, nothing to process
            if (latestFrame == null)
                return null;

            // Alerts allowed here
            return await BuildHeatmapFromFrameAsync(latestFrame, generateAlert: true);
        }

        /// <summary>
        /// Returns a heatmap for a specific frame
        /// </summary>
        public async Task<HeatmapResult?> GetHeatmapForFrameAsync(int frameId)
        {
            // Load the frame, including its DataFile relationship if needed
            var frame = await _context.PressureFrames
                .Include(f => f.DataFile)
                .FirstOrDefaultAsync(f => f.FrameId == frameId);

            // If frame doesn't exist
            if (frame == null)
                return null;

            // No alerts should be generated when navigating by slider
            return await BuildHeatmapFromFrameAsync(frame, generateAlert: false);
        }

        /// <summary>
        /// Core worker: load the 32x32 matrix for a frame, compute metrics,
        /// Raise an alert, and return HeatmapResult
        /// </summary>
        private async Task<HeatmapResult> BuildHeatmapFromFrameAsync(PressureFrame frame, bool generateAlert)
        {
            // Ensure the navigation property is loaded
            if (frame.DataFile == null)
                throw new InvalidOperationException("PressureFrame.DataFile must be included.");

            var path = frame.DataFile.FilePath;

            // Validate that the CSV file path exists and is valid
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                throw new FileNotFoundException($"CSV file not found: {path}");

            // Load the 32x32 matrix for the specific frame index
            int[,] matrix = await LoadMatrixForFrameAsync(path, frame.FrameIndex);

            // Perform analysis calculations on the matrix
            var peak = _analysis.CalculatePeakPressure(matrix);
            var avg = _analysis.CalculateAveragePressure(matrix);
            var contact = _analysis.CalculateContactAreaPercent(matrix);
            var risk = _analysis.CalculateRiskScore(peak, contact);

            // ============================
            // AUTO ALERT GENERATION
            // ============================
            const int highPressureThreshold = 180;

            // Generate an alert only when enabled AND peak pressure exceeds threshold
            if (generateAlert && peak > highPressureThreshold)
            {
                // Build a new alert record describing the high-pressure event
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

                // Save alert to the database
                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();
            }

            // Build and return the heatmap result object
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
            // Load all CSV lines into memory
            var lines = await File.ReadAllLinesAsync(filePath);

            const int rowsPerFrame = 32;
            const int cols = 32;

            // Calculate the starting row for this frame
            int startRow = frameIndex * rowsPerFrame;

            // Ensure the file actually contains enough rows for the requested frame
            if (lines.Length < startRow + rowsPerFrame)
                throw new InvalidOperationException("CSV does not contain enough rows for this frame.");

            // Allocate 32×32 matrix
            int[,] matrix = new int[rowsPerFrame, cols];

            // Parse each row of the frame into the matrix
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
