using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Tag
{
    public record AddTagDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; init; } = string.Empty;
    }
}
