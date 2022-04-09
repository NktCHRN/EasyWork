namespace WebAPI.DTOs
{
    public record UserMiniReducedDTO
    {
        public int Id { get; init; }

        public string FullName { get; init; } = string.Empty;
    }
}
