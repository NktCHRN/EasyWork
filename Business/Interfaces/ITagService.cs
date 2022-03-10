using Business.Models;

namespace Business.Interfaces
{
    public interface ITagService : ICRUD<TagModel>, IModelValidator<TagModel>
    {
        Task<IEnumerable<TagModel>> GetProjectTagsAsync(int projectId);

        Task<IEnumerable<TagModel>> GetTaskTagsAsync(int projectId);
    }
}
