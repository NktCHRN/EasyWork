using Business.Models;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IProjectService : ICRUD<ProjectModel>, IModelValidator<ProjectModel>
    {
        int GetCount();

        IEnumerable<ProjectModel> GetUserProjects(int userId); // (as a participant or owner); sorted by uop id reversed

        Task<ProjectModel?> GetProjectByActiveInviteCodeAsync(Guid inviteCode);

        Task<ProjectModel?> GetProjectByActiveInviteCodeAsync(string? inviteCode);

        Task<ProjectLimitsModel?> GetLimitsByIdAsync(int id);

        Task UpdateLimitsByIdAsync(int id, ProjectLimitsModel limits);
    }
}
