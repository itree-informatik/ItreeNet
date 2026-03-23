using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class ArbeitszeitService : IArbeitszeitService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public ArbeitszeitService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<List<Arbeitszeit>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tAll = await context.TArbeitszeit
                .AsNoTracking()
                .OrderByDescending(a => a.Jahr)
                .ThenBy(a => a.Monat)
                .ToListAsync();

            return _mapper.Map<List<Arbeitszeit>>(tAll);
        }

        public async Task<List<Arbeitszeit>> SaveAsync(Arbeitszeit model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TArbeitszeit>(model);
                context.TArbeitszeit.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TArbeitszeit>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }

        public async Task<List<Arbeitszeit>> DeleteAsync(Arbeitszeit model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tModel = _mapper.Map<TArbeitszeit>(model);
            context.TArbeitszeit.Remove(tModel);
            await context.SaveChangesAsync();

            return await GetAllAsync();
        }
    }
}
