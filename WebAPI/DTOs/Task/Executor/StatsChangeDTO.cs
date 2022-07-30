namespace WebAPI.DTOs.Task.Executor
{
    public record StatsChangeDTO
    {
        public int ProjectId { get; init; }

        public int UserId { get; init; }

        public short Value { get; init; }
    }
}
