using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.User
{
    public record ForgotPasswordDTO
    {
        [Required(AllowEmptyStrings = false)]
        [EmailAddress]
        public string Email { get; init; } = string.Empty;
        [Required(AllowEmptyStrings = false)]
        public string ClientURI { get; init; } = string.Empty;
    }
}
