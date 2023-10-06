using Projecttask.Models;

namespace Projecttask.Services.Interfaces;

public interface IMailService
{
    void SendEmail(Message message);
}
