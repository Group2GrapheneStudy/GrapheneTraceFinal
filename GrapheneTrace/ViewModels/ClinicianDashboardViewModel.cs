using System;

namespace GrapheneTrace.ViewModels
{
    public class ClinicianDashboardViewModel
    {
        public string ClinicianName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int AssignedPatientsCount { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
        public DateTime? NextAppointmentAt { get; set; }

        public int UnresolvedAlertsCount { get; set; }
        public int FeedbackToReviewCount { get; set; }
    }
}
