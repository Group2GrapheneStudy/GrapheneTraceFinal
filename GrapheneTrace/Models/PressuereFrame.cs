using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class PressureFrame
    {
        [Key]
        public int FrameId { get; set; }

        [ForeignKey(nameof(DataFile))]
        public int DataFileId { get; set; }

        public DataFile DataFile { get; set; } = null!;

        /// <summary>
        /// Index of the frame within the CSV session (0, 1, 2, ...).
        /// </summary>
        public int FrameIndex { get; set; }

        public DateTime CapturedAtUtc { get; set; }

        public decimal PeakPressure { get; set; }

        public decimal AveragePressure { get; set; }

        /// <summary>
        /// Contact area as a percentage (0-100).
        /// </summary>
        public decimal ContactAreaPercent { get; set; }

        /// <summary>
        /// A calculated risk score from your analysis service.
        /// </summary>
        public decimal RiskScore { get; set; }

        // Alerts triggered by this frame
        public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    }
}
