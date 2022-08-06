namespace WebAPI.DTOs.Task
{
    public record UserTaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTimeOffset StartDate { get; init; }

        public DateTimeOffset? Deadline { get; init; }

        public DateTimeOffset? EndDate { get; init; }

        public string Status { get; init; } = string.Empty;

        public int ProjectId { get; init; }
    }
}
