using Business.Models;

namespace Business.Interfaces
{
    public interface IReleaseService : ICRUD<ReleaseModel>, IModelValidator<ReleaseModel>
    {
        IEnumerable<ReleaseModel> GetProjectReleases(int projectId);
    }
}
