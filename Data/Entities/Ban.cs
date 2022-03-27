using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Ban
    {
        public int Id { get; set; }

        [Required]
        public DateTime DateFrom { get; set; }

        [Required]
        public DateTime DateTo { get; set; }

        [Column(TypeName = "nvarchar(400)")]
        public string? Hammer { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [InverseProperty("Bans")]
        public User? User { get; set; }

        public int? AdminId { get; set; }
        [ForeignKey("AdminId")]
        [InverseProperty("GivenBans")]
        public User? Admin { get; set; }
    }
}
