using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class FileModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(256, ErrorMessage = "The file name is too long")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int TaskId { get; set; }
    }
}
