using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        [ForeignKey(nameof(Clinician))]
        public int ClinicianId { get; set; }

        public Clinician Clinician { get; set; } = null!;

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedByUserId { get; set; }

        public UserAccount CreatedByUser { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(200)]
        public string MedicationName { get; set; } = null!;

        [MaxLength(200)]
        public string? Dosage { get; set; }

        [MaxLength(1000)]
        public string? Instructions { get; set; }
    }
}
