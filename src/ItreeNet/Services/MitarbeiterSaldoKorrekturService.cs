using ItreeNet.Data.Models.DB;
using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class MitarbeiterSaldoKorrekturService : IMitarbeiterSaldoKorrekturService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public MitarbeiterSaldoKorrekturService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<MitarbeiterSaldoKorrektur>> GetAllAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tMitarbeiterSaldoKorrektur = await context.TMitarbeiterSaldoKorrektur
                .AsNoTracking()
                .Include(k => k.CreatedByNavigation)
                .Where(f => f.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(f => f.Jahr).ThenByDescending(f => f.Monat)
                .ToListAsync();

            return tMitarbeiterSaldoKorrektur.Select(x => x.ToModel()).ToList();
        }

        public async Task<List<MitarbeiterSaldoKorrektur>> SaveAsync(MitarbeiterSaldoKorrektur model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = model.ToEntity();
                context.TMitarbeiterSaldoKorrektur.Add(tModel);
            }
            else
            {
                var tModel = model.ToEntity();
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync(model.MitarbeiterId);
        }
    }
}
