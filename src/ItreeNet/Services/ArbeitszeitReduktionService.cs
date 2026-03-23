using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class ArbeitszeitReduktionService : IArbeitszeitReduktionService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public ArbeitszeitReduktionService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<List<ArbeitszeitReduktion>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TArbeitszeitReduktion
                .AsNoTracking()
                .OrderByDescending(a => a.Datum)
                .ToListAsync();

            return _mapper.Map<List<ArbeitszeitReduktion>>(tAll);
        }

        public async Task<List<ArbeitszeitReduktion>> SaveAsync(ArbeitszeitReduktion model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TArbeitszeitReduktion>(model);
                context.TArbeitszeitReduktion.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TArbeitszeitReduktion>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();
            return await GetAllAsync();
        }

        public async Task<List<ArbeitszeitReduktion>> DeleteAsync(ArbeitszeitReduktion model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tModel = _mapper.Map<TArbeitszeitReduktion>(model);
            context.TArbeitszeitReduktion.Remove(tModel);
            await context.SaveChangesAsync();

            return await GetAllAsync();
        }
    }
}
