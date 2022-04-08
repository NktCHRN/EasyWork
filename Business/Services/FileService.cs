using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Other;
using Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using File = Data.Entities.File;

namespace Business.Services
{
    public class FileService : IFileService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        private readonly IFileManager _manager;

        public FileService(ApplicationDbContext context, IMapper mapper, IFileManager manager)
        {
            _context = context;
            _mapper = mapper;
            _manager = manager;
        }

        private async Task<File> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Files.FindAsync(id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(FileModel model, IFormFile file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));
            if (!IsValid(model, out string? error))
                throw new ArgumentException(error, nameof(model));
            const ushort maxFiles = 10;
                var task = await _context.Tasks.Include(m => m.Files).FirstAsync(m => m.Id == model.TaskId);
                if (task.Files.Count >= maxFiles)
                    throw new InvalidOperationException("Task can have not more than 10 files");
            var mapped = _mapper.Map<File>(model);
            await _context.Files.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
            await _manager.AddFileAsync(file, mapped.Id.ToString() + Path.GetExtension(model.Name), Enums.EasyWorkFileTypes.File);
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Files.Remove(model);
            await _context.SaveChangesAsync();
            try
            {
                _manager.DeleteFile(id.ToString() + Path.GetExtension(model.Name), Enums.EasyWorkFileTypes.File);
            }
            catch (Exception) { }
        }

        public async Task<FileModel?> GetByIdAsync(int id)
        {
            return _mapper.Map<FileModel?>(await _context.Files.FindAsync(id));
        }

        public async Task<IEnumerable<FileModelExtended>> GetTaskFilesAsync(int taskId)
        {
            var files = _mapper.Map<IEnumerable<FileModelExtended>>(_context.Files.Where(f => f.TaskId == taskId));
            foreach (var file in files)
                file.Size = await _manager.GetFileSizeAsync(file.Id.ToString() + Path.GetExtension(file.Name), Enums.EasyWorkFileTypes.File);
            return files;
        }

        public bool IsValid(FileModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<FileModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (!_context.Tasks.Any(t => t.Id == model.TaskId))
            {
                    firstErrorMessage = "Task with such an id was not found";
                    return false;
            }
            return true;
        }
    }
}
