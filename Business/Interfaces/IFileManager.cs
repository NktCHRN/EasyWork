using Business.Enums;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IFileManager
    {
        string? GetSolutionPath();

        string GetPathByEWType(string name, EasyWorkFileTypes ewtype);

        Task AddFileAsync(IFormFile file, string name, EasyWorkFileTypes ewtype);

        void DeleteFile(string name, EasyWorkFileTypes ewtype);

        string? GetImageMIMEType(string type);

        bool IsValidImageType(string type);

        FileStream GetFileStream(string name, EasyWorkFileTypes ewtype);

        byte[] GetFileContent(string name, EasyWorkFileTypes ewtype);
    }
}
