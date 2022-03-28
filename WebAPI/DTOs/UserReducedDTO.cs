namespace WebAPI.DTOs
{
    public record UserReducedDTO
    {
        byte[]? Avatar { get; init; }

        string? MIMEAvatarType { get; init; }

        string FullName { get; init; } = string.Empty;
    }
}
