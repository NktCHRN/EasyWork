using WebAPI.DTOs.Task;

namespace WebAPI.DTOs.Project.Tasks
{
    public record TasksDTO
    {
        public IEnumerable<TaskReducedDTO> ToDo { get; init; } = new List<TaskReducedDTO>();

        public IEnumerable<TaskReducedDTO> InProgress { get; init; } = new List<TaskReducedDTO>();

        public IEnumerable<TaskReducedDTO> Validate { get; init; } = new List<TaskReducedDTO>();

        public IEnumerable<TaskReducedDTO> Done { get; init; } = new List<TaskReducedDTO>();
    }
}
