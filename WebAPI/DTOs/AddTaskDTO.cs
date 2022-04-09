using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record AddTaskDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Status { get; set; } = string.Empty;
    }
}
