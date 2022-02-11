using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class MessageModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = null!;

        [Required]
        public bool IsReturnMessage { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int SenderId { get; set; }


        public ICollection<int> FilesIds { get; set; } = null!;
    }
}
