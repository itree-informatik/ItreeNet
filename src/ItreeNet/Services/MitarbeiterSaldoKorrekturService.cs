using AutoMapper;
using ItreeNet.Data.Models.DB;
using ItreeNet.Data.Models;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class MitarbeiterSaldoKorrekturService : IMitarbeiterSaldoKorrekturService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public MitarbeiterSaldoKorrekturService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
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

            return _mapper.Map<List<MitarbeiterSaldoKorrektur>>(tMitarbeiterSaldoKorrektur);
        }

        public async Task<List<MitarbeiterSaldoKorrektur>> SaveAsync(MitarbeiterSaldoKorrektur model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TMitarbeiterSaldoKorrektur>(model);
                context.TMitarbeiterSaldoKorrektur.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TMitarbeiterSaldoKorrektur>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync(model.MitarbeiterId);
        }
    }
}
