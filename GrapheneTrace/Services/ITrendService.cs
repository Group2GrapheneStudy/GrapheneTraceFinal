using System.Collections.Generic;
using System.Threading.Tasks;
using GrapheneTrace.Models;

namespace GrapheneTrace.Services
{
    public interface ITrendService
    {
        /// <summary>
        /// Returns a list of pressure trend points for the given patient
        /// over the last N hours (default 24). Uses real PressureFrame data.
        /// </summary>
        Task<List<TrendPoint>> GetPatientTrendAsync(int patientId, int lastHours = 24);

        /// <summary>
        /// Convenience wrapper used by PatientDataController.Trend().
        /// Uses a sensible default window (e.g. last 24 hours).
        /// </summary>
        Task<List<TrendPoint>> GetPatientPressureTrendAsync(int patientId);
    }
}
