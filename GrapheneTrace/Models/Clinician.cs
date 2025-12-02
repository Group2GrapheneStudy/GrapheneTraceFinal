using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Clinician
    {
        [Key]
        public int ClinicianId { get; set; }

        [ForeignKey(nameof(UserAccount))]
        public int UserId { get; set; }

        public UserAccount UserAccount { get; set; } = null!;

        [MaxLength(100)]
        public string? RegistrationNumber { get; set; }

        [MaxLength(200)]
        public string? Specialty { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Relationships
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
