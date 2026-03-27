using ItreeNet.Data.Models;
namespace ItreeNet.Interfaces;

public interface IReleaseService
{
    Task<List<Release>> GetAllAsync(DateOnly? von = null, DateOnly? bis = null);
    Task<List<Release>> GetNextPerAppAsync();
    Task<List<Release>> SaveAsync(Release release);
    Task<List<Release>> DeleteAsync(Release release);
}
