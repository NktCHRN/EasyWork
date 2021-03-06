namespace WebAPI.DTOs
{
    public record TaskDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public DateTime StartDate { get; init; }

        public DateTime? Deadline { get; init; }

        public DateTime? EndDate { get; init; }

        public string Status { get; set; } = string.Empty;

        public string? Priority { get; set; }
    }
}
