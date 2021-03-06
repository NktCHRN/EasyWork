namespace WebAPI.DTOs
{
    public record FileModelDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public long Size { get; init; }
    }
}
