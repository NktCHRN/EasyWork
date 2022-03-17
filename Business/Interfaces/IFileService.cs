using Business.Models;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IFileService : IModelValidator<FileModel>
    {
        Task<FileModel> GetByIdAsync(int id);

        Task AddAsync(FileModel model, IFormFile file);

        Task DeleteByIdAsync(int id);

        Task<IEnumerable<FileModel>> GetMessageFilesAsync(int messageId);

        Task<IEnumerable<FileModel>> GetTaskFilesAsync(int taskId);
    }
}
