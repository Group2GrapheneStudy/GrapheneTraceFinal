using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class DataFile
    {
        [Key]
        public int DataFileId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public Patient Patient { get; set; } = null!;

        [ForeignKey(nameof(UploadedByUser))]
        public int UploadedByUserId { get; set; }

        public UserAccount UploadedByUser { get; set; } = null!;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Path on disk or relative project path to the CSV file.
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = null!;

        [MaxLength(100)]
        public string? SourceDevice { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // Relationships
        public ICollection<PressureFrame> Frames { get; set; } = new List<PressureFrame>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
