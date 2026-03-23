using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class SpesenService : ISpesenService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public SpesenService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
            
        }

        public async Task<List<int>> GetSpesenYearsAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var yearsList = await (from e in context.TSpesen.AsNoTracking()
                            where e.MitarbeiterId == mitarbeiterId
                            group e by e.Datum.Year into g
                            orderby g.Key
                            select g).Select(x => x.Key).ToListAsync();

            return yearsList.OrderByDescending(x => x).ToList();
        }

        public async Task<List<Spesen>> GetSpesenMitarbeiterIdAsync(Guid mitarbeiterId, int year)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var startDate = new DateOnly(year, 1, 1);
            var endDate = new DateOnly(year, 12, 31);

            var list = await context.TSpesen
                .AsNoTracking()
                .Where(s => s.MitarbeiterId == mitarbeiterId && s.Datum >= startDate && s.Datum <= endDate)
                .OrderBy(s => s.Datum)
                .ToListAsync();

            return _mapper.Map<List<Spesen>>(list);
        }

        public async Task<List<Spesen>> InsertSpesenAsync(Spesen spesen)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            spesen.Id = Guid.NewGuid();
            var tSpesen = _mapper.Map<TSpesen>(spesen);
            context.TSpesen.Add(tSpesen);
            await context.SaveChangesAsync();

            return await GetSpesenMitarbeiterIdAsync(spesen.MitarbeiterId, spesen.Datum.Year);
        }

        public async Task<List<Spesen>> UpdateSpesenAsync(Spesen spesen)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tSpesen = _mapper.Map<TSpesen>(spesen);

            context.Entry(tSpesen).State = EntityState.Modified;
            await context.SaveChangesAsync();

            return await GetSpesenMitarbeiterIdAsync(spesen.MitarbeiterId, spesen.Datum.Year);
        }

        public async Task<List<Spesen>> DeleteSpesenAsync(Spesen spesen)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tSpesen = _mapper.Map<TSpesen>(spesen);

            context.TSpesen.Remove(tSpesen);
            await context.SaveChangesAsync();
            return await GetSpesenMitarbeiterIdAsync(spesen.MitarbeiterId, spesen.Datum.Year);
        }
    }
}
