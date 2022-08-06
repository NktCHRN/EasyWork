using Data.Entities;

namespace Business.Other
{
    public static class HelperMethods
    {
        public static bool IsDoneTask(TaskStatuses status) => status >= TaskStatuses.Validate;
    }
}
