using System.Security;
using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class KundenService : IKundenService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;
        private readonly UserService _userService;
        private readonly IMitarbeiterService _mitarbeiterService;

        public KundenService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper, UserService userService, IMitarbeiterService mitarbeiterService)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
            _userService = userService;
            _mitarbeiterService = mitarbeiterService;
        }
        public async Task<List<Kunde>> GetAllAsync(bool includeInactive = false)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tKunden = await context.TKunde
                                            .AsNoTracking()
                                            .Where(k => includeInactive || k.Aktiv == true)
                                            .OrderBy(k => k.Kundenname)
                                            .ToListAsync();

            return _mapper.Map<List<Kunde>>(tKunden);
        }

        public async Task<List<Kunde>> GetAllActiveAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tKunden = await context.TKunde
                    .AsNoTracking()
                    .Where(x => x.Aktiv == true)
                    .OrderBy(k => k.Kundenname)
                    .ToListAsync();

            return _mapper.Map<List<Kunde>>(tKunden);
        }

        public async Task<List<Kunde>> GetAllActiveOfTeamAsync(Guid? mitarbeiterId, bool allCustomers = false)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (_userService.CurrentUser == null || _userService.CurrentUser.MitarbeiterId == null ||
                _userService.CurrentUser.MitarbeiterId == Guid.Empty)
            {
                throw new SecurityException("CurrentUser nicht gefunden");
            }

            if (mitarbeiterId == null)
            {
                mitarbeiterId = _userService.CurrentUser.MitarbeiterId;
            }

            var mitarbeiter = await _mitarbeiterService.GetMitarbeiterByIdAsync((Guid)mitarbeiterId);

            var tKunden = await context.TKunde
                    .AsNoTracking()
                    .Where(x => x.Aktiv == true)
                    .OrderBy(k => k.Kundenname)
                    .ToListAsync();

            if (mitarbeiter.TeamIds != null && mitarbeiter.TeamIds.Any() && !allCustomers)
            {
                //var teamId = team.Id;
                tKunden = tKunden.Where(x =>
                    (x.TeamId != null && mitarbeiter.TeamIds.Contains(x.TeamId.Value)) || x.TeamId == null).ToList();
            }

            return _mapper.Map<List<Kunde>>(tKunden);
        }

        public async Task<Kunde> GetAsync(Guid kundenId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tKunde = await context.TKunde.AsNoTracking().SingleOrDefaultAsync(x => x.Id == kundenId)
                ?? throw new InvalidDataException($"Kunde {kundenId} nicht gefunden");

            return _mapper.Map<Kunde>(tKunde);
        }

        public async Task<List<Kunde>> SaveAsync(Kunde model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.Aktiv = true;

                var tModel = _mapper.Map<TKunde>(model);
                context.TKunde.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TKunde>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }

        public async Task<Kunde> SaveSingleAsync(Kunde model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.Aktiv = true;

                var tModel = _mapper.Map<TKunde>(model);
                context.TKunde.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TKunde>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return model;
        }

        public async Task<List<Kunde>> DeleteAsync(Kunde model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var hasProjekte = await context.TProjekt.AnyAsync(p => p.KundeId == model.Id);

            if (hasProjekte)
            {
                model.Aktiv = false;
                var tModel = _mapper.Map<TKunde>(model);
                context.Entry(tModel).State = EntityState.Modified;
                await context.SaveChangesAsync();

                var aktiveProjektIds = await context.TProjekt
                    .Where(p => p.KundeId == model.Id && p.Aktiv)
                    .Select(p => p.Id)
                    .ToListAsync();

                if (aktiveProjektIds.Any())
                {
                    await context.TProjekt
                        .Where(p => aktiveProjektIds.Contains(p.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.Aktiv, false));

                    await context.TVorgang
                        .Where(v => aktiveProjektIds.Contains(v.ProjektId) && v.Aktiv)
                        .ExecuteUpdateAsync(s => s.SetProperty(v => v.Aktiv, false));
                }
            }
            else
            {
                var tModel = _mapper.Map<TKunde>(model);
                context.TKunde.Remove(tModel);
                await context.SaveChangesAsync();
            }

            return await GetAllAsync();
        }
    }
}
