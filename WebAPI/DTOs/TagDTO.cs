namespace WebAPI.DTOs
{
    public record TagDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;
    }
}
