using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        /// <summary>
        /// The clinician this feedback is addressed to (if any).
        /// </summary>
        [ForeignKey(nameof(Clinician))]
        public int? ClinicianId { get; set; }

        public Clinician? Clinician { get; set; }

        /// <summary>
        /// Optional link to a specific data file / session.
        /// </summary>
        [ForeignKey(nameof(DataFile))]
        public int? DataFileId { get; set; }

        public DataFile? DataFile { get; set; }

        [MaxLength(5)]
        public string? Rating { get; set; }  // e.g. "1-5"

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this feedback is visible to the clinician.
        /// </summary>
        public bool VisibleToClinician { get; set; } = true;

        /// <summary>
        /// Clinician's reply, if any.
        /// </summary>
        [MaxLength(2000)]
        public string? ClinicianReply { get; set; }

        public DateTime? ClinicianReplyAt { get; set; }
    }
}
