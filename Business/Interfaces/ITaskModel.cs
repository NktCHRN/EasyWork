using Business.Models;

namespace Business.Interfaces
{
    public interface ITaskModel : ICRUD<TaskModel>, IModelValidator<TaskModel>
    {
        Task<IEnumerable<TaskModel>> GetProjectTasksAsync(int projectId);

        Task<IEnumerable<TaskModel>> GetUserTasksAsync(int userId);

        Task<IEnumerable<TaskModel>> GetUserOnProjectTasksAsync(int userOnProjectId);

        Task AddTagToTaskAsync(int taskId, TagModel tag);

        Task DeleteTagFromTaskAsync(int taskId, int tagId);
    }
}
