using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IUserAvatarService // Add UserManager to constructor!!!
    {
        Task UpdateAvatarAsync(int userId, IFormFile image);

        Task DeleteAvatarByUserIdAsync(int userId);
    }
}
