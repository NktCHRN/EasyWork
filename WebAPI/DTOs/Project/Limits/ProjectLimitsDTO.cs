using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs.Project.Limits
{
    public record ProjectLimitsDTO
    {
        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxToDo { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxInProgress { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "Only positive numbers and zero allowed")]
        public int? MaxValidate { get; init; }
    }
}
