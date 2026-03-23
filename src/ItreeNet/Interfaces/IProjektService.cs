using ItreeNet.Data.Enums;
using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IProjektService
    {
        Task<List<Projekt>> GetAllAsync();
        Task<List<Projekt>> GetAllActiveAsync();
        Task<List<Projekt>> ProjectsByCustomerIdAsync(Guid? clientId, EnumStatus status = EnumStatus.Aktiv);
        Task<List<Projekt>> AllProjectsByCustomerIdAsync(Guid? clientId);
        Task<List<Projekt>> SaveAsync(Projekt model);
        Task<List<Projekt>> DeleteAsync(Projekt model);
        Task<Projekt> SaveSingleAsync(Projekt model);
    }
}
