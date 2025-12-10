using GrapheneTrace.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [ForeignKey(nameof(UserAccount))]
        public int UserId { get; set; }

        public UserAccount UserAccount { get; set; } = null!;

        [MaxLength(100)]
        public string? ExternalId { get; set; }

        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Free-text notes about risk or conditions.
        /// </summary>
        [MaxLength(1000)]
        public string? RiskNotes { get; set; }

        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(200)]
        public string? ContactEmail { get; set; }

        // Relationships
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<DataFile> DataFiles { get; set; } = new List<DataFile>();
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
