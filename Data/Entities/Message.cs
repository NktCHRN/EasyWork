using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = null!;

        [Required]
        public bool IsEdited { get; set; }

        [Required]
        public bool IsRead { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int TaskId { get; set; }
        public Task? Task { get; set; }

        [Required]
        public int SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User? Sender { get; set; }
    }
}
