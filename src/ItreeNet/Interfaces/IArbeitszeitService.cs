using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces;

public interface IArbeitszeitService
{
    Task<List<Arbeitszeit>> GetAllAsync();
    Task<List<Arbeitszeit>> SaveAsync(Arbeitszeit model);
    Task<List<Arbeitszeit>> DeleteAsync(Arbeitszeit model);
}