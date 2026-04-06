using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class VorgangService : IVorgangService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public VorgangService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<List<Vorgang>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TVorgang
                .AsNoTracking()
                .Include(v => v.Projekt)
                .ThenInclude(p => p.Kunde)
                .ToListAsync();

            var all = _mapper.Map<List<Vorgang>>(tAll);
            return await GetBookedTimeOfVorgang(all);
        }

        public async Task<List<Vorgang>> GetAllActiveAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TVorgang
                .AsNoTracking()
                .Include(v => v.Projekt)
                .ThenInclude(p => p.Kunde)
                .Where(x => x.Aktiv == true)
                .ToListAsync();

            var all = _mapper.Map<List<Vorgang>>(tAll);
            return await GetBookedTimeOfVorgang(all);
        }

        public async Task<List<Vorgang>> GetAllProjecIdAsync(Guid? projectId, bool nurAktive = true)
        {
            if (projectId == null)
            {
                return new List<Vorgang>();
            }

            await using var context = await _dbFactory.CreateDbContextAsync();

            var vorgaenge = await context.TVorgang
                .AsNoTracking()
                .Include(v => v.Projekt)
                .ThenInclude(p => p.Kunde)
                .Where(x => x.ProjektId == projectId)
                .OrderBy(v => v.Bezeichnung)
                .ToListAsync();

            if (nurAktive)
            {
                vorgaenge = vorgaenge.Where(x => x.Aktiv).ToList();
            }

            var all = _mapper.Map<List<Vorgang>>(vorgaenge);
            return await GetBookedTimeOfVorgang(all);
        }

        public async Task<Vorgang> GetAsync(Guid vorgangId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tVorgang = await context.TVorgang
                .AsNoTracking()
                .Include(v => v.Projekt)
                .ThenInclude(p => p.Kunde)
                .SingleOrDefaultAsync(v => v.Id == vorgangId)
                ?? throw new InvalidDataException($"Vorgang {vorgangId} nicht gefunden");

            var model = _mapper.Map<Vorgang>(tVorgang);
            model.GebuchteStunden = await GetBookedTimeVorgangAsync(model.Id);

            return model;
        }

        public async Task<Kunde> GetKundeFromVorgangIdAsnyc(Guid vorgangId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tVorgang = await context.TVorgang
                .AsNoTracking()
                .Include(p => p.Projekt)
                .ThenInclude(k => k.Kunde)
                .SingleOrDefaultAsync(v => v.Id == vorgangId)
                ?? throw new InvalidDataException($"Vorgang {vorgangId} nicht gefunden");

            return _mapper.Map<Kunde>(tVorgang.Projekt.Kunde);
        }

        public async Task<Projekt> GetProjektFromVorgangIdAsync(Guid vorgangId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tVorgang = await context.TVorgang
                .AsNoTracking()
                .Include(p => p.Projekt)
                .ThenInclude(k => k.Kunde)
                .SingleOrDefaultAsync(v => v.Id == vorgangId)
                ?? throw new InvalidDataException($"Vorgang {vorgangId} nicht gefunden");

            return _mapper.Map<Projekt>(tVorgang.Projekt);
        }

        public async Task<List<Vorgang>> SaveAsync(Vorgang model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TVorgang>(model);
                context.TVorgang.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TVorgang>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }

        public async Task<Vorgang> SaveSingleAsync(Vorgang model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TVorgang>(model);
                context.TVorgang.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TVorgang>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return model;
        }

        public async Task<List<Vorgang>> DeleteAsync(Vorgang model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var anyBuchungen = await context.TBuchung.AnyAsync(b => b.VorgangId == model.Id);

            if (anyBuchungen)
            {
                model.Aktiv = false;
                var tModel = _mapper.Map<TVorgang>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }
            else
            {
                var tModel = _mapper.Map<TVorgang>(model);
                context.TVorgang.Remove(tModel);
            }
            
            await context.SaveChangesAsync();

            return await GetAllProjecIdAsync(model.ProjektId, false);
        }

        public async Task<decimal> GetBookedTimeVorgangAsync(Guid vorgangId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var bookedMinutes = await context.TBuchung
                .AsNoTracking()
                .Where(b => b.VorgangId == vorgangId)
                .SumAsync(b => b.Zeit);

            return bookedMinutes.HasValue ? bookedMinutes.Value / 60m : decimal.Zero;
        }

        private async Task<List<Vorgang>> GetBookedTimeOfVorgang(List<Vorgang> list)
        {
            if (list.Count == 0) return list;

            await using var context = await _dbFactory.CreateDbContextAsync();

            var ids = list.Select(v => v.Id).ToList();
            var bookedMinutesDict = await context.TBuchung
                .AsNoTracking()
                .Where(b => ids.Contains(b.VorgangId))
                .GroupBy(b => b.VorgangId)
                .Select(g => new { VorgangId = g.Key, Minuten = g.Sum(b => b.Zeit) })
                .ToDictionaryAsync(x => x.VorgangId, x => (x.Minuten ?? 0) / 60m);

            foreach (var item in list)
                item.GebuchteStunden = bookedMinutesDict.TryGetValue(item.Id, out var h) ? h : decimal.Zero;

            return list;
        }
    }
}
