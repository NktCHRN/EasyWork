using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Task
{
    public record AddTaskDTO
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string Status { get; init; } = string.Empty;
    }
}
