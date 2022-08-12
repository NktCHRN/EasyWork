using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.UserOnProject
{
    public record UserOnProjectDTO : UserOnProjectMiniDTO
    {
        [Required]
        public string Role { get; init; } = string.Empty;
    }
}
