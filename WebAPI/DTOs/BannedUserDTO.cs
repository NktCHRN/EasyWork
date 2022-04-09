namespace WebAPI.DTOs
{
    public record BannedUserDTO
    {
        public DateTime DateFrom { get; init; }

        public DateTime DateTo { get; init; }

        public string? Hammer { get; init; }

        public UserMiniDTO? Admin { get; init; }
    }
}
