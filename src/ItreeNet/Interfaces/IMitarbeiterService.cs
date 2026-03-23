using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IMitarbeiterService
    {
        Task<List<Mitarbeiter>> GetAllAsync(bool includeInactive = false);
        Task<List<Mitarbeiter>> GetAllActiveAsync(Guid? TeamId = null);
        Task<Mitarbeiter> GetMitarbeiterByIdAsync(Guid id);
        Task<List<Mitarbeiter>> SaveAsync(Mitarbeiter model);
        Task<Mitarbeiter> SaveSingleAsync(Mitarbeiter model);
        Task<List<Guid>> GetTeamIds(Guid mitarbeiterId);
    }
}
