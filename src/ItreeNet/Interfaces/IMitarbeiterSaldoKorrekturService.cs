using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IMitarbeiterSaldoKorrekturService
    {
        Task<List<MitarbeiterSaldoKorrektur>> GetAllAsync(Guid mitarbeiterId);
        Task<List<MitarbeiterSaldoKorrektur>> SaveAsync(MitarbeiterSaldoKorrektur model);
    }
}
