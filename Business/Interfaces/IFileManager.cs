﻿using Business.Enums;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IFileManager
    {
        string? GetSolutionPath();

        string GetPathByEWType(string name, EasyWorkFileTypes ewtype);

        Task AddFileAsync(IFormFile file, string? name, EasyWorkFileTypes ewtype);

        Task AddFileAsync(byte[] file, string name, EasyWorkFileTypes ewtype);

        void DeleteFile(string name, EasyWorkFileTypes ewtype);

        string? GetImageMIMEType(string type);

        string? GetImageType(string MIMEtype);

        bool IsValidImageType(string type);

        FileStream GetFileStream(string name, EasyWorkFileTypes ewtype);

        public Task<byte[]> GetFileContentAsync(string name, EasyWorkFileTypes ewtype);
    }
}
