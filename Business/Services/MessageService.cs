using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public MessageService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Message> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Messages.SingleOrDefaultAsync(m => m.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(MessageModel model)
        {
            model.Date = DateTime.Now;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var mapped = _mapper.Map<Message>(model);
            await _context.Messages.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Messages.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<MessageModel?> GetByIdAsync(int id)
        {
            return _mapper.Map<MessageModel?>(await _context.Messages.SingleOrDefaultAsync(m => m.Id == id));
        }

        public IEnumerable<MessageModel> GetTaskMessages(int taskId)
        {
            return _mapper.Map<IEnumerable<MessageModel>>(_context.Messages.Where(m => m.TaskId == taskId));
        }

        public bool IsValid(MessageModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<MessageModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (!_context.Tasks.Any(t => t.Id == model.TaskId))
            {
                firstErrorMessage = "The task with such an id was not found";
                return false;
            }
            if (!_context.Users.Any(u => u.Id == model.SenderId))
            {
                firstErrorMessage = "The user with such an id was not found";
                return false;
            }
            return true;
        }

        public async Task UpdateAsync(MessageModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.TaskId != existingModel.TaskId)
                throw new ArgumentException("Task cannot be changed", nameof(model));
            if (model.SenderId != existingModel.SenderId)
                throw new ArgumentException("Sender cannot be changed", nameof(model));
            if (model.Date != existingModel.Date)
                throw new ArgumentException("Date cannot be changed", nameof(model));
            if (model.IsRead == false && existingModel.IsRead)
                throw new ArgumentException("The \"read\" status cannot be returned to false", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.Messages.Update(existingModel);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<MessageModel> GetNotReadMessagesForUser(int userId)
        {
            var tasksIds = _context.Tasks.Where(t => t.ExecutorId == userId).Select(t => t.Id);
            var messages = _context.Messages.Where(m => tasksIds.Contains(m.TaskId) && m.SenderId != userId && !m.IsRead);
            return _mapper.Map<IEnumerable<MessageModel>>(messages).Reverse();
        }
    }
}
