using Business.Models;

namespace Business.Interfaces
{
    public interface ITagService : ICRUD<TagModel>, IModelValidator<TagModel>
    {
        IEnumerable<TagModel> GetProjectTags(int projectId);

        Task<IEnumerable<TagModel>> GetTaskTagsAsync(int taskId);

        Task<TagModel?> FindByName(string name);

        Task DeleteFromProjectByIdAsync(int id, int projectId);
    }
}
