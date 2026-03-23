using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IMailService
    {
        Task SendBookingNotificationAsync(Projekt project, decimal prozent);
    }
}
