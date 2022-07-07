namespace WebAPI.DTOs.Project
{
    public record ProjectReducedDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }
    }
}
