namespace WebAPI.DTOs.Task
{
    public record TaskReducedDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTimeOffset StartDate { get; init; }

        public DateTimeOffset? Deadline { get; init; }

        public DateTimeOffset? EndDate { get; init; }

        public string? Priority { get; init; }

        public int MessagesCount { get; init; }

        public int FilesCount { get; init; }
    }
}
