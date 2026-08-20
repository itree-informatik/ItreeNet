using AutoMapper;
using ItreeNet.Data.Enums;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace ItreeNet.Services
{
    public class AnwesenheitService : IAnwesenheitService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private readonly IPensumService _pensumService;

        public AnwesenheitService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper, UserService userService, IPensumService pensumService)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
            _userService = userService;
            _pensumService = pensumService;
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

        public async Task<AnwesenheitBulkResult> InsertAnwesenheitenAsync(Anwesenheit vorlage, DateOnly datumBis)
        {
            // security
            var currentUser = _userService.CurrentUser ?? throw new InvalidDataException("CurrentUser nicht gefunden");
            if (!currentUser.IsAdmin && currentUser.MitarbeiterId != vorlage.MitarbeiterId)
            {
                throw new SecurityException("Du bist nicht berechtigt, für einen anderen Mitarbeiter Anwesenheiten zu erfassen");
            }

            if (datumBis < vorlage.Datum)
            {
                throw new InvalidDataException("Datum bis darf nicht vor Datum von liegen");
            }

            var tagesStatus = await _pensumService.GetArbeitstageImZeitraumAsync(vorlage.MitarbeiterId, vorlage.Datum, datumBis);

            await using var context = await _dbFactory.CreateDbContextAsync();

            var vorhandeneDaten = await context.TAnwesenheit
                .AsNoTracking()
                .Where(a => a.MitarbeiterId == vorlage.MitarbeiterId && a.Typ == vorlage.Typ
                            && a.Datum >= vorlage.Datum && a.Datum <= datumBis)
                .Select(a => a.Datum)
                .ToHashSetAsync();

            var result = new AnwesenheitBulkResult();

            for (var tag = vorlage.Datum; tag <= datumBis; tag = tag.AddDays(1))
            {
                if (tagesStatus[tag] == EnumArbeitstagStatus.Feiertag)
                {
                    result.Uebersprungen.Add(new AnwesenheitBulkSkip { Datum = tag, Grund = EnumAnwesenheitBulkSkipGrund.Feiertag });
                    continue;
                }

                if (tagesStatus[tag] == EnumArbeitstagStatus.KeinArbeitstag)
                {
                    result.Uebersprungen.Add(new AnwesenheitBulkSkip { Datum = tag, Grund = EnumAnwesenheitBulkSkipGrund.KeinArbeitstag });
                    continue;
                }

                if (vorhandeneDaten.Contains(tag))
                {
                    result.Uebersprungen.Add(new AnwesenheitBulkSkip { Datum = tag, Grund = EnumAnwesenheitBulkSkipGrund.BereitsVorhanden });
                    continue;
                }

                context.TAnwesenheit.Add(new TAnwesenheit
                {
                    Id = Guid.NewGuid(),
                    MitarbeiterId = vorlage.MitarbeiterId,
                    Datum = tag,
                    Typ = vorlage.Typ,
                    Zeit = vorlage.Zeit,
                    // ZeitVon/ZeitBis bleiben null — im Mehrtages-Modus wird nur das Stundenfeld erfasst
                    Notiz = vorlage.Notiz
                });
                result.AnzahlErstellt++;
            }

            if (result.AnzahlErstellt > 0)
            {
                await context.SaveChangesAsync();
            }

            return result;
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

        public async Task<List<Anwesenheit>> SucheAnwesenheitAsync(Guid? mitarbeiterId, DateOnly? von, DateOnly? bis)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            IQueryable<TAnwesenheit> query = context.TAnwesenheit
                .AsNoTracking()
                .Include(a => a.Mitarbeiter);

            if (mitarbeiterId != null)
                query = query.Where(a => a.MitarbeiterId == mitarbeiterId);

            if (von != null)
                query = query.Where(a => a.Datum >= von);

            if (bis != null)
                query = query.Where(a => a.Datum <= bis);

            var tResult = await query.OrderByDescending(a => a.Datum).Take(1000).ToListAsync();

            return _mapper.Map<List<Anwesenheit>>(tResult);
        }
    }
}
