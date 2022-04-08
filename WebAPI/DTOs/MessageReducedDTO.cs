namespace WebAPI.DTOs
{
    public record MessageReducedDTO
    {
        public int Id { get; init; }

        public string Text { get; init; } = string.Empty;

        public int TaskId { get; init; }

        public string TaskName { get; init; } = string.Empty;

        public string SenderFullName { get; init; } = string.Empty;
    }
}
