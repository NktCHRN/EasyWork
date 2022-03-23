using Business.Models;

namespace Business.Interfaces
{
    public interface ITaskService : ICRUD<TaskModel>, IModelValidator<TaskModel>
    {
        IEnumerable<TaskModel> GetProjectTasks(int projectId);

        IEnumerable<TaskModel> GetUserTasks(int userId);

        IEnumerable<TaskModel> GetProjectUserTasks(int projectId, int userId);

        Task AddTagToTaskAsync(int taskId, int tagId);

        Task DeleteTagFromTaskAsync(int taskId, int tagId);
    }
}
