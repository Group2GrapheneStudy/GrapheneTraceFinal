namespace GrapheneTrace.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalPatients { get; set; }
        public int TotalClinicians { get; set; }
        public int TotalAlerts { get; set; }
        public int TotalAppointments { get; set; }
        public int TotalFeedback { get; set; }

        public List<GrapheneTrace.Models.Alert> RecentAlerts { get; set; } = new();
        public List<GrapheneTrace.Models.Feedback> RecentFeedback { get; set; } = new();
        public List<GrapheneTrace.Models.Appointment> RecentAppointments { get; set; } = new();
    }
}
