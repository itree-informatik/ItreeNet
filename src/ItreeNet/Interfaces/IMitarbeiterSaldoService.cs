using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IMitarbeiterSaldoService
    {
        Task<List<MitarbeiterSaldo>> GetAllAsync(Guid mitarbeiterId);
        Task<List<MitarbeiterSaldo>> SaveAsync(MitarbeiterSaldo model);
    }
}
