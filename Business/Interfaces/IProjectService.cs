using Business.Models;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IProjectService : ICRUD<ProjectModel>, IModelValidator<ProjectModel>
    {
        IEnumerable<ProjectModel> GetAll();

        IEnumerable<ProjectModel> GetUserProjects(int userId); // (as a participant or owner); sorted by project Id

        Task ChangeOwnerByProjectIdAsync(int projectId, int newOwnerId);        // new owner should already be a manager on the project

        Task UpdateMainPictureByProjectIdAsync(int projectId, IFormFile image);

        Task DeleteMainPictureByProjectIdAsync(int projectId);
    }
}
