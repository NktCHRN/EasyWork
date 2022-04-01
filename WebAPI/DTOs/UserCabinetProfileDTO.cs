namespace WebAPI.DTOs
{
    public record UserCabinetProfileDTO
    {
        public string Email { get; init; } = string.Empty;

        public string? PhoneNumber { get; init; }

        public string FirstName { get; init; } = string.Empty;

        public string? LastName { get; init; }

        public DateTime RegistrationDate { get; init; }

        public byte[]? Avatar { get; init; }

        public string? MIMEAvatarType { get; init; }

        public DateTime LastSeen { get; init; }

        public int TasksDone { get; init; }

        public int TasksNotDone { get; init; }

        public int Projects { get; init; }
    }
}
