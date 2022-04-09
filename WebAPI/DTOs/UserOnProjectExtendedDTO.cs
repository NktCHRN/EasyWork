namespace WebAPI.DTOs
{
    public record UserOnProjectExtendedDTO
    {
        public UserMiniWithAvatarDTO User { get; init; } = new UserMiniWithAvatarDTO();

        public string Role { get; init; } = string.Empty;

        public int TasksDone { get; init; }

        public int TasksNotDone { get; init; }
    }
}
