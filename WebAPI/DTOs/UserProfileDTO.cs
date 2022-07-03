namespace WebAPI.DTOs
{
    public record UserProfileDTO
    {
        public int Id { get; init; }

        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string? LastName { get; init; }

        public string? AvatarURL { get; init; }

        public string? MIMEAvatarType { get; init; }

        public DateTimeOffset? LastSeen { get; init; }

        public int TasksDone { get; init; }

        public int TasksNotDone { get; init; }

        public int Projects { get; init; }
    }
}
