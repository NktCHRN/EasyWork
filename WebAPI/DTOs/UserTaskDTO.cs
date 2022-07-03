namespace WebAPI.DTOs
{
    public record UserTaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTimeOffset StartDate { get; init; }

        public DateTimeOffset? Deadline { get; init; }

        public string Status { get; set; } = string.Empty;
    }
}
