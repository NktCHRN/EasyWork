using Business.Models;
using Data.Entities;
using Task = System.Threading.Tasks.Task;

namespace Business.Interfaces
{
    public interface ITaskService : ICRUD<TaskModel>, IModelValidator<TaskModel>
    {
        IEnumerable<TaskModel> GetProjectTasksByDate(int projectId, DateTime from, DateTime to);

        IEnumerable<TaskModel> GetProjectTasksByStatusAndTag(int projectId, TaskStatuses status, int? tagId = null);

        IEnumerable<TaskModel> GetUserTasks(int userId);

        Task AddTagToTaskAsync(int taskId, int tagId);

        Task DeleteTagFromTaskAsync(int taskId, int tagId);
    }
}
