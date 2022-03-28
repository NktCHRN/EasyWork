using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IUserAvatarService
    {
        Task UpdateAvatarAsync(int userId, IFormFile image);

        Task UpdateAvatarAsync(int userId, byte[] image, string imageType);

        Task DeleteAvatarByUserIdAsync(int userId);
    }
}
