namespace WebAPI.DTOs
{
    public record UserTaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTime StartDate { get; init; }

        public DateTime? Deadline { get; init; }

        public string Status { get; set; } = string.Empty;
    }
}
