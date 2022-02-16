using Business.Models;

namespace Business.Interfaces
{
    public interface ITagService : ICRUD<TagModel>
    {
        Task<IAsyncEnumerable<TagModel>> GetProjectTagsAsync(int projectId);

        Task<IAsyncEnumerable<TagModel>> GetTaskTagsAsync(int projectId);
    }
}
