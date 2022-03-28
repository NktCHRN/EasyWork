using Business.Models;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IProjectService : ICRUD<ProjectModel>, IModelValidator<ProjectModel>
    {
        int GetCount();

        IEnumerable<ProjectModel> GetUserProjects(int userId); // (as a participant or owner); sorted by uop id reversed

        Task UpdateMainPictureByProjectIdAsync(int projectId, IFormFile image);

        Task DeleteMainPictureByProjectIdAsync(int projectId);
    }
}
