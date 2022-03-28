using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record LoginUserDTO
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
    }
}
