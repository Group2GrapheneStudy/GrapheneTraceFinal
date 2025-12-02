using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Alert
    {
        [Key]
        public int AlertId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        /// <summary>
        /// Optional link to the frame that caused the alert.
        /// </summary>
        [ForeignKey(nameof(Frame))]
        public int? FrameId { get; set; }

        public PressureFrame? Frame { get; set; }

        [ForeignKey(nameof(RaisedByUser))]
        public int RaisedByUserId { get; set; }

        public UserAccount RaisedByUser { get; set; } = null!;

        [ForeignKey(nameof(ResolvedByUser))]
        public int? ResolvedByUserId { get; set; }

        public UserAccount? ResolvedByUser { get; set; }

        [MaxLength(20)]
        public string Severity { get; set; } = "Medium";   // e.g. Low / Medium / High

        [MaxLength(100)]
        public string AlertType { get; set; } = "HighPressure";

        [MaxLength(50)]
        public string Status { get; set; } = "Open";       // Open / Acknowledged / Resolved

        [MaxLength(1000)]
        public string? Message { get; set; }

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }
    }
}
