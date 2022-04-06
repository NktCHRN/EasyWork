using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record AddReleaseDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }
    }
}
