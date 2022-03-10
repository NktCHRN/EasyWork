using Business.Models;

namespace Business.Interfaces
{
    public interface IProjectService : ICRUD<ProjectModel>, IModelValidator<ProjectModel>
    {
        Task<IEnumerable<ProjectModel>> GetAllAsync();

        Task<IEnumerable<ProjectModel>> GetUserProjectsAsync(int userId); // (as participant)
    }
}
