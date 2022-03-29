using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.DTOs
{
    public record RegisterUserDTO
    {
        [Required(AllowEmptyStrings = false)]
        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, ErrorMessage = "The maximum length of the first name is 50")]
        public string FirstName { get; init; } = string.Empty;

        [Column(TypeName = "nvarchar(50)")]
        [StringLength(50, ErrorMessage = "The maximum length of the last name is 50")]
        public string? LastName { get; init; }

        [Required(AllowEmptyStrings = false)]
        public string Email { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Password { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string PasswordConfirm { get; init; } = string.Empty;
    }
}
