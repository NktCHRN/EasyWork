using Business.Models;

namespace Business.Interfaces
{
    public interface IMessageService : ICRUD<MessageModel>, IModelValidator<MessageModel>
    {
        Task<IAsyncEnumerable<MessageModel>> GetTaskMessagesAsync(int taskId);
    }
}
