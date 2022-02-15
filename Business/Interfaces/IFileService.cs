using Business.Models;

namespace Business.Interfaces
{
    public interface IFileService
    {
        Task<FileModel> GetByIdAsync(int id);

        Task AddAsync(FileModel model);

        Task DeleteByIdAsync(int id);

        Task<IAsyncEnumerable<FileModel>> GetMessageFilesAsync(int messageId);

        Task<IAsyncEnumerable<FileModel>> GetTaskFilesAsync(int taskId);
    }
}
