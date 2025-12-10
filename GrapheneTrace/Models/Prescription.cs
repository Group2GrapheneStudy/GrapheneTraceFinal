using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Clinician))]
        public int ClinicianId { get; set; }
        public Clinician Clinician { get; set; } = null!;

        [Required]
        public string DrugName { get; set; } = string.Empty;

        public string Dosage { get; set; } = string.Empty;

        public string Frequency { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Quantity { get; set; }

        public string? Instructions { get; set; }

        public DateTime DatePrescribed { get; set; } = DateTime.UtcNow;
    }
}
