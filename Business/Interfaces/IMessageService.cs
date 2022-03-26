using Business.Models;

namespace Business.Interfaces
{
    public interface IMessageService : ICRUD<MessageModel>, IModelValidator<MessageModel>
    {
        IEnumerable<MessageModel> GetTaskMessages(int taskId);

        IEnumerable<MessageModel> GetNotReadMessagesForUser(int userId);
    }
}
