namespace WebAPI.DTOs
{
    public record TaskReducedDTO
    {
        public int Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public DateTime StartDate { get; init; }

        public DateTime? Deadline { get; init; }

        public DateTime? EndDate { get; init; }

        public int MessagesCount { get; init; }

        public int FilesCount { get; init; }

        public UserMiniWithAvatarDTO? Executor { get; init; }
    }
}
