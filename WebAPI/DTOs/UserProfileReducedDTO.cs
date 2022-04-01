namespace WebAPI.DTOs
{
    public record UserProfileReducedDTO
    {
        public int Id { get; init; }

        public string FullName { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public DateTime? LastSeen { get; init; }

        public byte[]? Avatar { get; init; }

        public string? MIMEAvatarType { get; init; }
    }
}
