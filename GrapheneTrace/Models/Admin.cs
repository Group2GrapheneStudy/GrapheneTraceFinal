using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GrapheneTrace.Models
{
    public class Admin
    {
        [Key]
        public int AdminId { get; set; }

        [ForeignKey(nameof(UserAccount))]
        public int UserId { get; set; }

        public UserAccount UserAccount { get; set; } = null!;
    }
}
