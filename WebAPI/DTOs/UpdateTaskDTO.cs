using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record UpdateTaskDTO
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? Deadline { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Status { get; set; } = string.Empty;

        public string? Priority { get; set; }
    }
}
