using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        [ForeignKey(nameof(Clinician))]
        public int ClinicianId { get; set; }

        public Clinician Clinician { get; set; } = null!;

        /// <summary>
        /// Which user created this appointment (admin/clinician/patient).
        /// </summary>
        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedByUserId { get; set; }

        public UserAccount CreatedByUser { get; set; } = null!;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }
		[Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; //Default for patient self-booking

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
