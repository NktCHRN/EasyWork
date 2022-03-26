using Business.Models;

namespace Business.Interfaces
{
    public interface ITaskService : ICRUD<TaskModel>, IModelValidator<TaskModel>
    {
        IEnumerable<TaskModel> GetProjectTasksByDate(int projectId, DateTime from, DateTime to);

        IEnumerable<TaskModel> GetProjectNotArchivedTasks(int projectId);

        IEnumerable<TaskModel> GetProjectArchivedTasks(int projectId);

        IEnumerable<TaskModel> GetUserTasks(int userId);

        Task AddTagToTaskAsync(int taskId, int tagId);

        Task DeleteTagFromTaskAsync(int taskId, int tagId);
    }
}
