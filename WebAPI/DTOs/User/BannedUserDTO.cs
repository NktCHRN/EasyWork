namespace WebAPI.DTOs.User
{
    public record BannedUserDTO
    {
        public DateTimeOffset DateFrom { get; init; }

        public DateTimeOffset DateTo { get; init; }

        public string? Hammer { get; init; }

        public UserMiniDTO? Admin { get; init; }
    }
}
