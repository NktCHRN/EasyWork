using Business.Models;
using Data.Entities;
using Task = System.Threading.Tasks.Task;

namespace Business.Interfaces
{
    public interface ITaskService : ICRUD<TaskModel>, IModelValidator<TaskModel>
    {
        IEnumerable<TaskModel> GetProjectTasksByDate(int projectId, DateTime from, DateTime to);

        IEnumerable<TaskModel> GetProjectTasksByStatus(int projectId, TaskStatuses status);

        IEnumerable<TaskModel> GetUserTasks(int userId);

        Task AddTagToTaskAsync(int taskId, int tagId);

        Task DeleteTagFromTaskAsync(int taskId, int tagId);
    }
}
