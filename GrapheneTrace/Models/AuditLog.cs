using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditId { get; set; }

        [ForeignKey(nameof(UserAccount))]
        public int UserId { get; set; }

        public UserAccount UserAccount { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = null!;   // e.g. "CreatePatient", "UpdateAppointment"

        [MaxLength(100)]
        public string? EntityType { get; set; }       // e.g. "Patient", "Alert"

        public int? EntityId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(2000)]
        public string? Details { get; set; }
    }
}
