using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record LoginUserDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; init; } = null!;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; init; } = null!;
    }
}
