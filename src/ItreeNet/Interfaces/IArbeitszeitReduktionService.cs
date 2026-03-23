using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IArbeitszeitReduktionService
    {
        Task<List<ArbeitszeitReduktion>> GetAllAsync();
        Task<List<ArbeitszeitReduktion>> SaveAsync(ArbeitszeitReduktion model);
        Task<List<ArbeitszeitReduktion>> DeleteAsync(ArbeitszeitReduktion model);
    }
}
