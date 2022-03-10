using Business.Models;

namespace Business.Interfaces
{
    public interface IReleaseService : ICRUD<ReleaseModel>, IModelValidator<ReleaseModel>
    {
        Task<IEnumerable<ReleaseModel>> GetProjectReleasesAsync(int projectId);
    }
}
