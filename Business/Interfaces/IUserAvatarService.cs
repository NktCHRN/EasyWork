using Data.Entities;
using Microsoft.AspNetCore.Http;
using Task = System.Threading.Tasks.Task;

namespace Business.Interfaces
{
    public interface IUserAvatarService // Add UserManager to constructor!!!
    {
        Task UpdateAvatarAsync(User model, IFormFile image);

        Task DeleteAvatarByUserIdAsync(int userId);
    }
}
