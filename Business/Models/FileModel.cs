using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class FileModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "The file name is too long")]
        public string Name { get; set; } = null!;

        public int? TaskId { get; set; }

        public int? MessageId { get; set; }
    }
}
