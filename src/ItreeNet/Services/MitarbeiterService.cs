using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class MitarbeiterService : IMitarbeiterService
    {
        private readonly IMapper _mapper;
        private readonly  IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public MitarbeiterService(IMapper mapper, IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _mapper = mapper;
            _dbFactory = dbFactory;
        }

        public async Task<List<Mitarbeiter>> GetAllAsync(bool includeInactive = false)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TMitarbeiter
                .AsNoTracking()
                .Where(m => includeInactive || m.Aktiv == true)
                .OrderByDescending(m => m.Intern)
                .ThenBy(m => m.Nachname)
                .ToListAsync();

            var mitarbeiterIds = tAll.Select(m => m.Id).ToList();
            var allTeamIds = await context.TMitarbeiterTeam
                .Where(t => mitarbeiterIds.Contains(t.MitarbeiterId))
                .Select(t => new { t.MitarbeiterId, t.TeamId })
                .ToListAsync();

            var mitarbeiterList = _mapper.Map<List<Mitarbeiter>>(tAll);
            foreach (var mitarbeiter in mitarbeiterList)
                mitarbeiter.TeamIds = allTeamIds.Where(t => t.MitarbeiterId == mitarbeiter.Id).Select(t => t.TeamId).ToList();

            return mitarbeiterList;
        }

        public async Task<List<Mitarbeiter>> GetAllActiveAsync(Guid? teamId = null)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tAll = await context.TMitarbeiter
                .AsNoTracking()
                .Where(m => m.Aktiv == true)
                .OrderByDescending(m => m.Intern)
                .ThenBy(m => m.Vorname)
                .ToListAsync();

            if(teamId != null)
            {
                var teamMitglieder = await context.TMitarbeiterTeam.Where(m => m.TeamId == teamId)
                    .Select(t => t.MitarbeiterId).ToListAsync();

                tAll = tAll.Where(m => teamMitglieder.Contains(m.Id)).ToList();
            }

            var mitarbeiterIds = tAll.Select(m => m.Id).ToList();
            var allTeamIds = await context.TMitarbeiterTeam
                .Where(t => mitarbeiterIds.Contains(t.MitarbeiterId))
                .Select(t => new { t.MitarbeiterId, t.TeamId })
                .ToListAsync();

            var mitarbeiterList = _mapper.Map<List<Mitarbeiter>>(tAll);
            foreach (var mitarbeiter in mitarbeiterList)
                mitarbeiter.TeamIds = allTeamIds.Where(t => t.MitarbeiterId == mitarbeiter.Id).Select(t => t.TeamId).ToList();

            return mitarbeiterList;
        }

        public async Task<Mitarbeiter> GetMitarbeiterByIdAsync(Guid id)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tMitarbeiter = await context.TMitarbeiter
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tMitarbeiter == null)
            {
                throw new InvalidDataException("Employee not found");
            }

            var mitarbeiter = _mapper.Map<Mitarbeiter>(tMitarbeiter);
            mitarbeiter.TeamIds = await context.TMitarbeiterTeam
                .Where(m => m.MitarbeiterId == id)
                .Select(t => t.TeamId)
                .ToListAsync();

            return mitarbeiter;
        }

        public async Task<Mitarbeiter> SaveSingleAsync(Mitarbeiter model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.AzureId == string.Empty)
            {
                model.AzureId = null;
            }

            if (model.Id == Guid.Empty)
            {
                model.Aktiv = true;
                model.Id = Guid.NewGuid();
                var tModel = _mapper.Map<TMitarbeiter>(model);
                context.TMitarbeiter.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TMitarbeiter>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await CreateMitarbeiterTeam(context, model);

            await context.SaveChangesAsync();

            return model;
        }

        public async Task<List<Mitarbeiter>> SaveAsync(Mitarbeiter model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Aktiv = true;
                model.Id = Guid.NewGuid();
                var tModel = _mapper.Map<TMitarbeiter>(model);
                context.TMitarbeiter.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TMitarbeiter>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await CreateMitarbeiterTeam(context, model);

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }

        public async Task<List<Guid>> GetTeamIds(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var teamIds = await context.TMitarbeiterTeam.Where(m => m.MitarbeiterId == mitarbeiterId)
                .Select(t => t.TeamId).ToListAsync();

            return teamIds;
        }

        private async Task CreateMitarbeiterTeam(ZeiterfassungContext context, Mitarbeiter model)
        {
            var mitarbeiterTeams = await context.TMitarbeiterTeam.Where(t => t.MitarbeiterId == model.Id).ToListAsync();
            context.TMitarbeiterTeam.RemoveRange(mitarbeiterTeams);

            if (model.TeamIds != null)
            {
                foreach (var teamId in model.TeamIds)
                {
                    var mt = new TMitarbeiterTeam
                    {
                        Id = Guid.NewGuid(),
                        MitarbeiterId = model.Id,
                        TeamId = teamId
                    };
                    await context.TMitarbeiterTeam.AddAsync(mt);
                }
            }
        }
    }
}
