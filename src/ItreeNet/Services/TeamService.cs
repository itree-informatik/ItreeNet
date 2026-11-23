using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class TeamService : ITeamService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public TeamService(IDbContextFactory<ZeiterfassungContext> dbFactory) 
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Team>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TTeam
                .AsNoTracking()
                .OrderBy(a => a.Sort)
                .ToListAsync();

            return tAll.Select(x => x.ToModel()).ToList();
        }

        public async Task<List<Team>> GetAllActiveAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TTeam
                .AsNoTracking()
                .Where(w => w.Aktiv)
                .OrderBy(a => a.Sort)
                .ToListAsync();

            return tAll.Select(x => x.ToModel()).ToList();
        }

        public async Task<List<Team>> SaveAsync(Team model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = model.ToEntity();
                context.TTeam.Add(tModel);
            }
            else
            {
                var tModel = model.ToEntity();
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }

        public async Task<List<Team>> DeleteAsync(Team model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tMitarbeiter = await context.TMitarbeiterTeam.AnyAsync(p => p.TeamId == model.Id);
            var tKunde = await context.TKunde.AnyAsync(p => p.TeamId == model.Id);

            if (tMitarbeiter || tKunde)
            {
                model.Aktiv = false;
                var tModel = model.ToEntity();
                context.Entry(tModel).State = EntityState.Modified;
            }
            else
            {
                var tModel = model.ToEntity();
                context.TTeam.Remove(tModel);
            }

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }
    }
}
