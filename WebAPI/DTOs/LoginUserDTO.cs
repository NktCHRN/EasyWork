using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record LoginUserDTO
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password is required.")]
        public string Password { get; init; } = string.Empty;
    }
}
