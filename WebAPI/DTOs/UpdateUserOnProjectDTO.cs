using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record UpdateUserOnProjectDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Role { get; init; } = string.Empty;
    }
}
