using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IVorgangService
    {
        Task<List<Vorgang>> GetAllAsync();
        Task<List<Vorgang>> GetAllActiveAsync();
        Task<List<Vorgang>> GetAllProjecIdAsync(Guid? projectId, bool nurAktive = true);
        Task<Vorgang> GetAsync(Guid vorgangId);
        Task<Kunde> GetKundeFromVorgangIdAsnyc(Guid vorgangId);
        Task<Projekt> GetProjektFromVorgangIdAsync(Guid vorgangId);
        Task<List<Vorgang>> SaveAsync(Vorgang model);
        Task<List<Vorgang>> DeleteAsync(Vorgang model);
        Task<Vorgang> SaveSingleAsync(Vorgang model);
        Task<decimal> GetBookedTimeVorgangAsync(Guid vorgangId);
    }
}
