namespace WebAPI.DTOs
{
    public record GanttDTO
    {
        public DateTime StartDate { get; init; }

        public DateTime EndDate { get; init; }

        public IEnumerable<GanttTaskDTO> Tasks { get; init; } = new List<GanttTaskDTO>();
    }
}
