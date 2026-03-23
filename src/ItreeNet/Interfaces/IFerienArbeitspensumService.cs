using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IFerienArbeitspensumService
    {
        Task<List<FerienArbeitspensum>> GetAllAsync(Guid mitarbeiterId);
        Task<List<FerienArbeitspensum>> SaveAsync(FerienArbeitspensum model);
    }
}
