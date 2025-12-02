using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrapheneTrace.Models
{
    public class UserAccount
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string PasswordHash { get; set; } = null!;

        /// <summary>
        /// "Patient", "Clinician", or "Admin"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Navigation to role-specific records
        public Patient? Patient { get; set; }
        public Clinician? Clinician { get; set; }
        public Admin? Admin { get; set; }

        // Other relationships
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<Alert> AlertsRaised { get; set; } = new List<Alert>();
        public ICollection<Alert> AlertsResolved { get; set; } = new List<Alert>();
        public ICollection<DataFile> UploadedDataFiles { get; set; } = new List<DataFile>();
        public ICollection<Appointment> CreatedAppointments { get; set; } = new List<Appointment>();
        public ICollection<Prescription> CreatedPrescriptions { get; set; } = new List<Prescription>();
    }
}
