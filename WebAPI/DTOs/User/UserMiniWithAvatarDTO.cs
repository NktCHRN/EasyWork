namespace WebAPI.DTOs.User
{
    public record UserMiniWithAvatarDTO
    {
        public int Id { get; init; }

        public string FullName { get; init; } = string.Empty;

        public string? AvatarURL { get; init; }

        public string? MIMEAvatarType { get; init; }
    }
}
