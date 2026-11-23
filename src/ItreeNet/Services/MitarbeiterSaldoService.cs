using ItreeNet.Data.Models.DB;
using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class MitarbeiterSaldoService : IMitarbeiterSaldoService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public MitarbeiterSaldoService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<MitarbeiterSaldo>> GetAllAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tMitarbeiterSaldo = await context.TMitarbeiterSaldo
                .AsNoTracking()
                .Where(f => f.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(f => f.Jahr).ThenByDescending(f => f.Monat)
                .ToListAsync();

            return tMitarbeiterSaldo.Select(x => x.ToModel()).ToList();
        }

        public async Task<List<MitarbeiterSaldo>> SaveAsync(MitarbeiterSaldo model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = model.ToEntity();
                context.TMitarbeiterSaldo.Add(tModel);
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
