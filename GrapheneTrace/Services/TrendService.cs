using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrapheneTrace.Data;
using GrapheneTrace.Models;
using Microsoft.EntityFrameworkCore;

namespace GrapheneTrace.Services
{
    public class TrendService : ITrendService
    {
        private readonly AppDbContext _context;

        public TrendService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of pressure trend points for the given patient.
        /// This uses ONLY real PressureFrame data that came from the CSVs.
        ///
        /// We:
        ///  1. Get the patient's DataFiles.
        ///  2. Take up to the latest 2000 PressureFrames for those files.
        ///  3. Downsample to at most 'maxPoints' frames by picking evenly
        ///     spaced frames in time order (no averaging, no synthetic values).
        /// </summary>
        public async Task<List<TrendPoint>> GetPatientTrendAsync(int patientId, int maxPoints = 24)
        {
            var result = new List<TrendPoint>();

            try
            {
                // 1) All data files for this patient
                var dataFileIds = await _context.DataFiles
                    .Where(d => d.PatientId == patientId)
                    .Select(d => d.DataFileId)
                    .ToListAsync();

                if (!dataFileIds.Any())
                    return result;

                // 2) Latest frames for those files, ordered by actual capture time
                //    Limit total rows to avoid timeouts on very large datasets.
                var frames = await _context.PressureFrames
                    .AsNoTracking()
                    .Where(f => dataFileIds.Contains(f.DataFileId))
                    .OrderByDescending(f => f.CapturedAtUtc)
                    .Take(2000)
                    .OrderBy(f => f.CapturedAtUtc) // re-order ascending after Take
                    .ToListAsync();

                if (!frames.Any())
                    return result;

                // 3) Downsample to at most maxPoints frames by picking evenly spaced indices.
                //    We KEEP real timestamps and values – no averaging, no fake time.
                List<PressureFrame> selectedFrames = new List<PressureFrame>();

                if (frames.Count <= maxPoints)
                {
                    selectedFrames = frames;
                }
                else
                {
                    double step = (frames.Count - 1) / (double)(maxPoints - 1);

                    for (int i = 0; i < maxPoints; i++)
                    {
                        int index = (int)Math.Round(i * step);
                        if (index < 0) index = 0;
                        if (index >= frames.Count) index = frames.Count - 1;

                        selectedFrames.Add(frames[index]);
                    }
                }

                // 4) Map selected frames into TrendPoint objects.
                foreach (var f in selectedFrames)
                {
                    result.Add(new TrendPoint
                    {
                        Timestamp = f.CapturedAtUtc,          // REAL DB timestamp
                        PeakPressure = f.PeakPressure,        // REAL DB value
                        ContactArea = f.ContactAreaPercent    // REAL DB value
                    });
                }
            }
            catch (Exception)
            {
                // Fail gracefully – empty list → "Not enough data" in the view.
                return new List<TrendPoint>();
            }

            return result;
        }

        /// <summary>
        /// Convenience wrapper used by PatientDataController.Trend().
        /// Returns up to 24 real data points from the most recent frames.
        /// </summary>
        public Task<List<TrendPoint>> GetPatientPressureTrendAsync(int patientId)
        {
            return GetPatientTrendAsync(patientId, 24);
        }
    }
}
