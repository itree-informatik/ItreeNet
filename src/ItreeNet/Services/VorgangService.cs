using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class VorgangService : IVorgangService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public VorgangService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Vorgang>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TVorgang
                .AsNoTracking()
                .Include(v => v.Projekt)
                .ThenInclude(p => p.Kunde)
                .ToListAsync();

            var all = tAll.Select(x => x.ToModel()).ToList();
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

            var all = tAll.Select(x => x.ToModel()).ToList();
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

            var all = vorgaenge.Select(x => x.ToModel()).ToList();
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

            var model = tVorgang.ToModel();
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

            return tVorgang.Projekt.Kunde.ToModel();
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

            return tVorgang.Projekt.ToModel();
        }

        public async Task<List<Vorgang>> SaveAsync(Vorgang model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = model.ToEntity();
                context.TVorgang.Add(tModel);
            }
            else
            {
                var tModel = model.ToEntity();
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

                var tModel = model.ToEntity();
                context.TVorgang.Add(tModel);
            }
            else
            {
                var tModel = model.ToEntity();
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
                var tModel = model.ToEntity();
                context.Entry(tModel).State = EntityState.Modified;
            }
            else
            {
                var tModel = model.ToEntity();
                context.TVorgang.Remove(tModel);
            }
            
            await context.SaveChangesAsync();

            return await GetAllProjecIdAsync(model.ProjektId, false);
        }

        public async Task<decimal> GetBookedTimeVorgangAsync(Guid vorgangId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var bookedHours = await context.TBuchung
                .AsNoTracking()
                .Where(b => b.VorgangId == vorgangId)
                .SumAsync(b => b.Zeit);

            if (bookedHours == null)
            {
                return decimal.Zero;
            }
            return (decimal)bookedHours;
        }

        private async Task<List<Vorgang>> GetBookedTimeOfVorgang(List<Vorgang> list)
        {
            if (list.Count == 0) return list;

            await using var context = await _dbFactory.CreateDbContextAsync();

            var ids = list.Select(v => v.Id).ToList();
            var bookedHoursDict = await context.TBuchung
                .AsNoTracking()
                .Where(b => ids.Contains(b.VorgangId))
                .GroupBy(b => b.VorgangId)
                .Select(g => new { VorgangId = g.Key, Stunden = g.Sum(b => b.Zeit) })
                .ToDictionaryAsync(x => x.VorgangId, x => x.Stunden ?? decimal.Zero);

            foreach (var item in list)
                item.GebuchteStunden = bookedHoursDict.TryGetValue(item.Id, out var h) ? h : decimal.Zero;

            return list;
        }
    }
}
