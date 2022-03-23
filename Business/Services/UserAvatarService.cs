﻿using Business.Interfaces;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class UserAvatarService : IUserAvatarService
    {
        private readonly ApplicationDbContext _context;

        private readonly IFileManager _manager;

        public UserAvatarService(ApplicationDbContext context, IFileManager manager)
        {
            _context = context;
            _manager = manager;
        }

        private async Task<User> GetNotMappedByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user is null)
                throw new InvalidOperationException("User with such an id was not found");
            return user;
        }

        public async Task DeleteAvatarByUserIdAsync(int userId)
        {
            var user = await GetNotMappedByIdAsync(userId);
            if (string.IsNullOrEmpty(user.AvatarFormat))
                throw new InvalidOperationException("The project does not have any main picture right now");
            try
            {
                _manager.DeleteFile(user.Id.ToString() + '.' + user.AvatarFormat, Enums.EasyWorkFileTypes.UserAvatar);
                user.AvatarFormat = null;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
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
                throw new ArgumentException("Not appropriate file type", image.FileName);
            var newFileName = userId + extension;
            try
            {
                await _manager.AddFileAsync(image, newFileName, Enums.EasyWorkFileTypes.UserAvatar);
                if (oldFileName != newFileName)
                {
                    user.AvatarFormat = extension[1..];
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(oldFileName))
                        _manager.DeleteFile(oldFileName, Enums.EasyWorkFileTypes.UserAvatar);
                }
            }
            catch (Exception) { }
        }
    }
}
