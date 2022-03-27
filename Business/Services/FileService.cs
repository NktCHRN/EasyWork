﻿using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Microsoft.AspNetCore.Http;
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
            var mapped = _mapper.Map<File>(model);
            await _context.Files.AddAsync(mapped);
            await _context.SaveChangesAsync();
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

        public async Task<FileModel> GetByIdAsync(int id)
        {
            return _mapper.Map<FileModel>(await GetNotMappedByIdAsync(id));
        }

        public IEnumerable<FileModel> GetMessageFiles(int messageId)
        {
            return _mapper.Map<IEnumerable<FileModel>>(_context.Files.Where(f => f.MessageId == messageId));
        }

        public IEnumerable<FileModel> GetTaskFiles(int taskId)
        {
            return _mapper.Map<IEnumerable<FileModel>>(_context.Files.Where(f => f.TaskId == taskId));
        }

        public bool IsValid(FileModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<FileModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (model.TaskId is not null && model.MessageId is not null)
            {
                firstErrorMessage = "Only TaskId or MessageId should not be null";
                return false;
            }
            if (model.MessageId is not null && !_context.Messages.Any(m => m.Id == model.MessageId))
            {
                    firstErrorMessage = "Message with such an id was not found";
                    return false;
            }
            if (model.TaskId is not null && !_context.Tasks.Any(t => t.Id == model.TaskId))
            {
                    firstErrorMessage = "Task with such an id was not found";
                    return false;
            }
            return true;
        }
    }
}