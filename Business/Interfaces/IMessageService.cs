using Business.Models;

namespace Business.Interfaces
{
    public interface IMessageService : ICRUD<MessageModel>, IModelValidator<MessageModel>
    {
        Task<IEnumerable<MessageModel>> GetTaskMessagesAsync(int taskId);
    }
}
