namespace WebAPI.DTOs.User
{
    public record UserReducedDTO
    {
        public string? AvatarURL { get; init; }

        public string? MIMEAvatarType { get; init; }

        public string FullName { get; init; } = string.Empty;
    }
}
