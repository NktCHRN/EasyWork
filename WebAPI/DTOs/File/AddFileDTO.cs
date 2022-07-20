using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.File
{
    public record AddFileDTO
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(256, ErrorMessage = "The file name is too long")]
        public string Name { get; set; } = string.Empty;
    }
}
