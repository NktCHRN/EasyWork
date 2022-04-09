using Data.Entities;

namespace Business.Other
{
    public record UserOnProjectModelExtended
    {
        public int UserId { get; init; }

        public UserOnProjectRoles Role { get; init; }

        public int TasksDone { get; init; }

        public int TasksNotDone { get; init; }
    }
}
