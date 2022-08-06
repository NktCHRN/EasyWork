using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Message
{
    public record AddMessageDTO
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(2000, ErrorMessage = "Too long message. The maximum length is 2000 characters")]
        public string Text { get; init; } = string.Empty;
    }
}
