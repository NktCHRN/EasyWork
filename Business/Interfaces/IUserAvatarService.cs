using Microsoft.AspNetCore.Http;
using Business.Other;

namespace Business.Interfaces
{
    public interface IUserAvatarService
    {
        Task UpdateAvatarAsync(int userId, IFormFile image);

        Task UpdateAvatarAsync(int userId, byte[] image, string imageType);

        Task DeleteAvatarByUserIdAsync(int userId);

        Task<UserDossier?> GetDossierByIdAsync(int id);
    }
}
