using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class ArbeitszeitService : IArbeitszeitService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public ArbeitszeitService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<Arbeitszeit>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TArbeitszeit
                .AsNoTracking()
                .OrderByDescending(a => a.Jahr)
                .ThenBy(a => a.Monat)
                .ToListAsync();

            return tAll.Select(x => x.ToModel()).ToList();
        }

        public async Task<List<Arbeitszeit>> SaveAsync(Arbeitszeit model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = model.ToEntity();
                context.TArbeitszeit.Add(tModel);
            }
            else
            {
                var tModel = model.ToEntity();
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }

        public async Task<List<Arbeitszeit>> DeleteAsync(Arbeitszeit model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tModel = model.ToEntity();
            context.TArbeitszeit.Remove(tModel);
            await context.SaveChangesAsync();

            return await GetAllAsync();
        }
    }
}
