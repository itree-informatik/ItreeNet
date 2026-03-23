using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class AnwesenheitService : IAnwesenheitService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public AnwesenheitService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<List<Anwesenheit>> GetByMitarbeiterAsync(Guid mitarbeiterId, DateOnly von, DateOnly bis)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var records = await context.TAnwesenheit
                .AsNoTracking()
                .Include(a => a.Mitarbeiter)
                .Where(a => a.MitarbeiterId == mitarbeiterId && a.Datum >= von && a.Datum <= bis)
                .OrderBy(a => a.Datum)
                .ToListAsync();

            return _mapper.Map<List<Anwesenheit>>(records);
        }

        public async Task<Anwesenheit?> GetByIdAsync(Guid id)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var record = await context.TAnwesenheit
                .AsNoTracking()
                .Include(a => a.Mitarbeiter)
                .SingleOrDefaultAsync(a => a.Id == id);

            return record == null ? null : _mapper.Map<Anwesenheit>(record);
        }

        public async Task<Anwesenheit> SaveAsync(Anwesenheit model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                var tModel = _mapper.Map<TAnwesenheit>(model);
                context.TAnwesenheit.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TAnwesenheit>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();
            return model;
        }

        public async Task DeleteAsync(Guid id)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var record = await context.TAnwesenheit.SingleOrDefaultAsync(a => a.Id == id);
            if (record != null)
            {
                context.TAnwesenheit.Remove(record);
                await context.SaveChangesAsync();
            }
        }
    }
}
