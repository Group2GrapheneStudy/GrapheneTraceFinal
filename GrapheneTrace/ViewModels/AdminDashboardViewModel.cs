using System.Collections.Generic;
using GrapheneTrace.Models;

namespace GrapheneTrace.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<Patient> Patients { get; set; } = new();
        public List<Clinician> Clinicians { get; set; } = new();
        public List<Admin> Admins { get; set; } = new();
    }
}
