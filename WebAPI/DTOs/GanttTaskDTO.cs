namespace WebAPI.DTOs
{
    public record GanttTaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTime GanttStartDate { get; init; }

        public DateTime GanttDeadline { get; init; }

        public DateTime GanttEndDate { get; init; }

        public UserMiniReducedDTO? Executor { get; init; }
    }
}
