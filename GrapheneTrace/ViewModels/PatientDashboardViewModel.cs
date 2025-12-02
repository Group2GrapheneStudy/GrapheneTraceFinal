using System;

namespace GrapheneTrace.ViewModels
{
    public class PatientDashboardViewModel
    {
        public string PatientName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public int AlertCount { get; set; }
        public int OpenAlertCount { get; set; }
        public int FeedbackCount { get; set; }
        public int UpcomingAppointmentsCount { get; set; }
        public DateTime? NextAppointmentAt { get; set; }

        public bool HasPressureData { get; set; }
    }
}
