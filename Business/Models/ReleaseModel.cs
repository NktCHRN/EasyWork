using System.ComponentModel.DataAnnotations;

namespace Business.Models
{
    public class ReleaseModel
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTimeOffset Date { get; set; }

        [Required]
        public int ProjectId { get; set; }
    }
}
