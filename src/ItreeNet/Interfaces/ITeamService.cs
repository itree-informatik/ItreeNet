using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface ITeamService
    {
        Task<List<Team>> GetAllAsync();
        Task<List<Team>> GetAllActiveAsync();
        Task<List<Team>> SaveAsync(Team model);
        Task<List<Team>> DeleteAsync(Team model);
    }
}
