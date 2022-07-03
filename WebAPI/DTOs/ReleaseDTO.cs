namespace WebAPI.DTOs
{
    public record ReleaseDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public DateTimeOffset Date { get; init; }
    }
}
