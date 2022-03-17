using Business.Models;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IProjectService : ICRUD<ProjectModel>, IModelValidator<ProjectModel>
    {
        Task<IEnumerable<ProjectModel>> GetAllAsync();

        Task<IEnumerable<ProjectModel>> GetUserProjectsAsync(int userId); // (as participant)

        Task UpdateProjectMainPictureAsync(ProjectModel model, IFormFile image);

        Task DeleteProjectMainPictureAsync(ProjectModel model);
    }
}
