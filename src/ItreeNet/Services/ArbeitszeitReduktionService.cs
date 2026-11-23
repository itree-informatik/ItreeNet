using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class ArbeitszeitReduktionService : IArbeitszeitReduktionService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public ArbeitszeitReduktionService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<ArbeitszeitReduktion>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TArbeitszeitReduktion
                .AsNoTracking()
                .OrderByDescending(a => a.Datum)
                .ToListAsync();

            return tAll.Select(x => x.ToModel()).ToList();
        }

        public async Task<List<ArbeitszeitReduktion>> SaveAsync(ArbeitszeitReduktion model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = model.ToEntity();
                context.TArbeitszeitReduktion.Add(tModel);
            }
            else
            {
                var tModel = model.ToEntity();
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();
            return await GetAllAsync();
        }

        public async Task<List<ArbeitszeitReduktion>> DeleteAsync(ArbeitszeitReduktion model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tModel = model.ToEntity();
            context.TArbeitszeitReduktion.Remove(tModel);
            await context.SaveChangesAsync();

            return await GetAllAsync();
        }
    }
}
