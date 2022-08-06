using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class UserAvatarService : IUserAvatarService
    {
        private readonly UserManager<User> _userManager;

        private readonly IFileManager _manager;

        public UserAvatarService(UserManager<User> userManager, IFileManager manager)
        {
            _userManager = userManager;
            _manager = manager;
        }

        private async Task<User> GetNotMappedByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                throw new InvalidOperationException("User with such an id was not found");
            return user;
        }

        public async Task DeleteAvatarByUserIdAsync(int userId)
        {
            var user = await GetNotMappedByIdAsync(userId);
            if (string.IsNullOrEmpty(user.AvatarFormat))
                throw new InvalidOperationException("User does not have any avatar right now");
            try
            {
                _manager.DeleteFile(user.Id.ToString() + '.' + user.AvatarFormat, Enums.EasyWorkFileTypes.UserAvatar);
                user.AvatarFormat = null;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception) { }
        }

        public async Task UpdateAvatarAsync(int userId, IFormFile image)
        {
            var user = await GetNotMappedByIdAsync(userId);
            string? oldFileName = null;
            if (!string.IsNullOrEmpty(user.AvatarFormat))
                oldFileName = user.Id + "." + user.AvatarFormat;
            var extension = Path.GetExtension(image.FileName);
            if (!_manager.IsValidImageType(extension))
                throw new ArgumentException("Not appropriate file type", nameof(image));
            var newFileName = userId + extension;
                await _manager.AddFileAsync(image, newFileName, Enums.EasyWorkFileTypes.UserAvatar);
                if (oldFileName != newFileName)
                {
                    user.AvatarFormat = extension[1..];
                    await _userManager.UpdateAsync(user);
                    if (!string.IsNullOrEmpty(oldFileName))
                        _manager.DeleteFile(oldFileName, Enums.EasyWorkFileTypes.UserAvatar);
                }
        }

        public async Task UpdateAvatarAsync(int userId, byte[] image, string imageType)
        {
            var user = await GetNotMappedByIdAsync(userId);
            string? oldFileName = null;
            if (!string.IsNullOrEmpty(user.AvatarFormat))
                oldFileName = user.Id + "." + user.AvatarFormat;
            if (!_manager.IsValidImageType(imageType))
                throw new ArgumentException("Not appropriate file type", nameof(imageType));
            if (!imageType.StartsWith("."))
                imageType = "." + imageType;
            var newFileName = userId + imageType;
                await _manager.AddFileAsync(image, newFileName, Enums.EasyWorkFileTypes.UserAvatar);
                if (oldFileName != newFileName)
                {
                    user.AvatarFormat = imageType[1..];
                    await _userManager.UpdateAsync(user);
                    if (!string.IsNullOrEmpty(oldFileName))
                        _manager.DeleteFile(oldFileName, Enums.EasyWorkFileTypes.UserAvatar);
                }
        }
    }
}
