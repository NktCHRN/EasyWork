namespace WebAPI.DTOs.Task
{
    public record TaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public DateTimeOffset StartDate { get; init; }

        public DateTimeOffset? Deadline { get; init; }

        public DateTimeOffset? EndDate { get; init; }

        public string Status { get; set; } = string.Empty;

        public string? Priority { get; set; }

        public int ProjectId { get; set; }
    }
}
