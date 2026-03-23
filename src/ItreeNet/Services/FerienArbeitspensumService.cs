using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class FerienArbeitspensumService : IFerienArbeitspensumService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public FerienArbeitspensumService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<List<FerienArbeitspensum>> GetAllAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tFerienPensen = await context.TFerienArbeitspensum
                .AsNoTracking()
                .Where(f => f.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(f => f.GueltigAb)
                .ToListAsync();

            return _mapper.Map<List<FerienArbeitspensum>>(tFerienPensen);
        }
        public async Task<List<FerienArbeitspensum>> SaveAsync(FerienArbeitspensum model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TFerienArbeitspensum>(model);
                context.TFerienArbeitspensum.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TFerienArbeitspensum>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync(model.MitarbeiterId);
        }
    }
}
