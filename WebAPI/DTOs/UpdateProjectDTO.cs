using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public record UpdateProjectDTO
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(150)]
        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxToDo { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxInProgress { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxValidate { get; init; }

        public bool IsInviteCodeActive { get; init; }
    }
}
