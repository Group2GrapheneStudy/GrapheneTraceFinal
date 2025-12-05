using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        // -------------------------
        // PATIENT RELATION
        // -------------------------
        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        // -------------------------
        // CLINICIAN RELATION 
        // -------------------------
        [ForeignKey(nameof(Clinician))]
        public int? ClinicianId { get; set; }
        public Clinician? Clinician { get; set; }

        // -------------------------
        // DATA FILE / SESSION 
        // -------------------------
        [ForeignKey(nameof(DataFile))]
        public int? DataFileId { get; set; }
        public DataFile? DataFile { get; set; }

        // -------------------------
        // RATING & COMMENT
        // Rating is optional → int?
        // NULL = no rating given
        // 1–5 = valid rating
        // -------------------------
        public int? Rating { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // -------------------------
        // VISIBILITY / REPLY
        // -------------------------
        public bool VisibleToClinician { get; set; } = true;

        [MaxLength(2000)]
        public string? ClinicianReply { get; set; }

        public DateTime? ClinicianReplyAt { get; set; }
    }
}
