using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class FerienArbeitspensumService : IFerienArbeitspensumService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public FerienArbeitspensumService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<List<FerienArbeitspensum>> GetAllAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tFerienPensen = await context.TFerienArbeitspensum
                .AsNoTracking()
                .Where(f => f.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(f => f.GueltigAb)
                .ToListAsync();

            return tFerienPensen.Select(x => x.ToModel()).ToList();
        }
        public async Task<List<FerienArbeitspensum>> SaveAsync(FerienArbeitspensum model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = model.ToEntity();
                context.TFerienArbeitspensum.Add(tModel);
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
