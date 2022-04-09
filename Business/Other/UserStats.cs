namespace Business.Other
{
    public record UserStats
    {
        public int UserId { get; init; }

        public int TasksDone { get; init; }

        public int TasksNotDone { get; init; }

        public int Projects { get; init; }
    }
}
