namespace WebAPI.DTOs
{
    public record GanttTaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTimeOffset GanttStartDate { get; init; }

        public DateTimeOffset GanttDeadline { get; init; }

        public DateTimeOffset GanttEndDate { get; init; }
    }
}
