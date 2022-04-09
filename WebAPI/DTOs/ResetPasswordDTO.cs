using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record ResetPasswordDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Password { get; init; } = string.Empty;
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; init; } = string.Empty;
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;
        [Required(AllowEmptyStrings = false)]
        public string Token { get; init; } = string.Empty;
    }
}
