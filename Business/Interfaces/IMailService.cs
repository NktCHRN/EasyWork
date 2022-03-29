using Business.Other;

namespace Business.Interfaces
{
    public interface IMailService
    {
        Task SendAsync(MailRequest mailRequest);
    }
}
