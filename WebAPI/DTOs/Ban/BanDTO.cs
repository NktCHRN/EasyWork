using WebAPI.DTOs.User;

namespace WebAPI.DTOs.Ban
{
    public record BanDTO
    {
        public int Id { get; init; }

        public DateTimeOffset DateFrom { get; init; }

        public DateTimeOffset DateTo { get; init; }

        public string? Hammer { get; init; }

        public UserMiniDTO? Admin { get; init; }

        public UserMiniDTO User { get; init; } = null!;
    }
}
