using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record UpdateUserDTO
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, ErrorMessage = "The maximum length of the first name is 50")]
        public string FirstName { get; init; } = string.Empty;

        [StringLength(50, ErrorMessage = "The maximum length of the last name is 50")]
        public string? LastName { get; init; }

        [Phone]
        public string? PhoneNumber { get; init; }
    }
}
