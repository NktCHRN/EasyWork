using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Token
{
    public record ExternalAuthDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Provider { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string IdToken { get; init; } = string.Empty;
    }
}
