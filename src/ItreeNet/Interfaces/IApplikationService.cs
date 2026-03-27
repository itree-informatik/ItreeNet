using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces;

public interface IApplikationService
{
    Task<List<Applikation>> GetAllAsync();
    Task<List<Applikation>> SaveAsync(Applikation app);
    Task<List<Applikation>> DeleteAsync(Applikation app);
}
