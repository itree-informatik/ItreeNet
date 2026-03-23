using AutoMapper;
using ItreeNet.Data.Models.DB;
using ItreeNet.Data.Models;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class MitarbeiterSaldoService : IMitarbeiterSaldoService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public MitarbeiterSaldoService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<List<MitarbeiterSaldo>> GetAllAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tMitarbeiterSaldo = await context.TMitarbeiterSaldo
                .AsNoTracking()
                .Where(f => f.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(f => f.Jahr).ThenByDescending(f => f.Monat)
                .ToListAsync();

            return _mapper.Map<List<MitarbeiterSaldo>>(tMitarbeiterSaldo);
        }

        public async Task<List<MitarbeiterSaldo>> SaveAsync(MitarbeiterSaldo model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TMitarbeiterSaldo>(model);
                context.TMitarbeiterSaldo.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TMitarbeiterSaldo>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync(model.MitarbeiterId);
        }
    }
}
