namespace WebAPI.DTOs
{
    public record BanDTO
    {
        public int Id { get; init; }

        public DateTime DateFrom { get; init; }

        public DateTime DateTo { get; init; }

        public string? Hammer { get; init; }

        public UserMiniDTO? Admin { get; init; }

        public UserMiniDTO User { get; init; } = null!;
    }
}
