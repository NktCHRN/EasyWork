using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class Message
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public DateTimeOffset Date { get; set; }

        [Required]
        public int TaskId { get; set; }
        public Task? Task { get; set; }

        [Required]
        public int SenderId { get; set; }
        [ForeignKey("SenderId")]
        public User? Sender { get; set; }
    }
}
