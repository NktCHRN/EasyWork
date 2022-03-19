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
            var model = await _context.Messages.FindAsync(id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(MessageModel model)
        {
            model.Date = DateTime.Now;
            model.IsEdited = false;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            await _context.Messages.AddAsync(_mapper.Map<Message>(model));
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Messages.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<MessageModel> GetByIdAsync(int id)
        {
            var model = await _context.Messages.Include(m => m.Files).SingleOrDefaultAsync(m => m.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return _mapper.Map<MessageModel>(model);
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
            model.IsEdited = true;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.TaskId != existingModel.TaskId)
                throw new ArgumentException("Task cannot be changed", nameof(model));
            if (model.SenderId != existingModel.SenderId)
                throw new ArgumentException("Sender cannot be changed", nameof(model));
            if (model.IsReturnMessage != existingModel.IsReturnMessage)
                throw new ArgumentException("Message status cannot be changed", nameof(model));
            if (model.Date != existingModel.Date)
                throw new ArgumentException("Date cannot be changed", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.Messages.Update(existingModel);
            await _context.SaveChangesAsync();
        }
    }
}
