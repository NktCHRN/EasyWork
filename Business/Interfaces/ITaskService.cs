using Business.Models;
using Data.Entities;
using Task = System.Threading.Tasks.Task;

namespace Business.Interfaces
{
    public interface ITaskService : ICRUD<TaskModel>, IModelValidator<TaskModel>
    {
        IEnumerable<TaskModel> GetProjectTasksByDate(int projectId, DateTimeOffset from, DateTimeOffset to);

        IEnumerable<TaskModel> GetProjectTasksByStatus(int projectId, TaskStatuses status);

        IEnumerable<TaskModel> GetUserTasks(int userId);

        Task AddExecutorToTaskAsync(int taskId, int userId);

        Task DeleteExecutorFromTaskAsync(int taskId, int userId);

        IEnumerable<User> GetTaskExecutors(int taskId);
    }
}
