using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class MessageModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2000, ErrorMessage = "Too long message. The maximum length is 2000 characters")]
        public string Text { get; set; } = null!;

        [Required]
        public bool IsRead { get; set; }

        [Required]
        public bool IsEdited { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int SenderId { get; set; }
    }
}
