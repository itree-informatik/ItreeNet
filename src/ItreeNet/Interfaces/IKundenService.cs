using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IKundenService
    {
        Task<List<Kunde>> GetAllAsync(bool includeInactive = false);
        Task<List<Kunde>> GetAllActiveAsync();
        Task<List<Kunde>> SaveAsync(Kunde model);
        Task<List<Kunde>> DeleteAsync(Kunde model);
        Task<List<Kunde>> GetAllActiveOfTeamAsync(Guid? mitarbeiterId, bool allCustomers = false);
        Task<Kunde> GetAsync(Guid KundenId);
        Task<Kunde> SaveSingleAsync(Kunde model);
    }
}
