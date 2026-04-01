using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IAnwesenheitService
    {
        Task<List<Anwesenheit>> GetByMitarbeiterAsync(Guid mitarbeiterId, DateOnly von, DateOnly bis);
        Task<Anwesenheit?> GetByIdAsync(Guid id);
        Task<Anwesenheit> SaveAsync(Anwesenheit model);
        Task DeleteAsync(Guid id);
        Task<List<Anwesenheit>> SucheAnwesenheitAsync(Guid? mitarbeiterId, DateOnly? von, DateOnly? bis);
    }
}
