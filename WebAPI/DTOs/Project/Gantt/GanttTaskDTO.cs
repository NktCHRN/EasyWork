namespace WebAPI.DTOs.Project.Gantt
{
    public record GanttTaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public double Offset { get; init; }

        public double EndDateWidth { get; init; }
    }
}
