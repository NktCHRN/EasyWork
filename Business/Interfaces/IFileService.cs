﻿using Business.Models;
using Business.Other;
using Microsoft.AspNetCore.Http;

namespace Business.Interfaces
{
    public interface IFileService : IModelValidator<FileModel>
    {
        Task<FileModel?> GetByIdAsync(int id);

        Task AddAsync(FileModel model, IFormFile file);

        Task DeleteByIdAsync(int id);

        Task<IEnumerable<FileModelExtended>> GetTaskFilesAsync(int taskId);

        public Task ChunkAddStartAsync(FileModel model);

        public Task AddChunkAsync(int fileId, FileChunkModel chunkModel);

        public Task<FileModelExtended> ChunkAddEndAsync(int fileId);
    }
}
