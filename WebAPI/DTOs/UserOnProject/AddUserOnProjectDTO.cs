using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.UserOnProject
{
    public record AddUserOnProjectDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Role { get; init; } = string.Empty;

        [Required]
        public int UserId { get; init; }
    }
}
