using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.UserOnProject
{
    public record UpdateUserOnProjectDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Role { get; init; } = string.Empty;
    }
}
