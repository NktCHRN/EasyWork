namespace Business.Other
{
    public record UserDossier
    {
        public int Id { get; init; }

        public string FullName { get; init; } = string.Empty;

        public byte[]? Avatar { get; init; }

        public string? MIMEAvatarType { get; init; }
    }
}
