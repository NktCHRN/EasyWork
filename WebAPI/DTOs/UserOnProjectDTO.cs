using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record UserOnProjectDTO
    {
        [Required]
        public string Role { get; init; } = string.Empty;

        [Required]
        public int ProjectId { get; init; }

        [Required]
        public int UserId { get; init; }
    }
}
