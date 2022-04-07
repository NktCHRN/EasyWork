using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class MessageModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(2000, ErrorMessage = "Too long message. The maximum length is 2000 characters")]
        public string Text { get; set; } = string.Empty;

        [Required]
        public bool IsRead { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int SenderId { get; set; }
    }
}
