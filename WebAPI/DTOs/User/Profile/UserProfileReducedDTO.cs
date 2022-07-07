namespace WebAPI.DTOs.User.Profile
{
    public record UserProfileReducedDTO
    {
        public int Id { get; init; }

        public string FullName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public DateTimeOffset? LastSeen { get; init; }

        public string? AvatarURL { get; init; }

        public string? MIMEAvatarType { get; init; }
    }
}
