using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record AddTagDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; init; } = string.Empty;
    }
}
