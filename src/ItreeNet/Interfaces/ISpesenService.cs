using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface ISpesenService
    {
        Task<List<int>> GetSpesenYearsAsync(Guid mitarbeiterId);
        Task<List<Spesen>> GetSpesenMitarbeiterIdAsync(Guid mitarbeiterId, int year);
        Task<List<Spesen>> InsertSpesenAsync(Spesen spesen);
        Task<List<Spesen>> UpdateSpesenAsync(Spesen spesen);
        Task<List<Spesen>> DeleteSpesenAsync(Spesen deletedItem);
    }
}
