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

            await SaveWithCascadeAsync(context, model);

            return await GetAllAsync();
        }

        public async Task<Kunde> SaveSingleAsync(Kunde model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            await SaveWithCascadeAsync(context, model);

            return model;
        }

        private async Task SaveWithCascadeAsync(ZeiterfassungContext context, Kunde model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.Aktiv = true;

                var tNeu = _mapper.Map<TKunde>(model);
                context.TKunde.Add(tNeu);

                await context.SaveChangesAsync();
                return;
            }

            var bisher = await context.TKunde
                .AsNoTracking()
                .Where(k => k.Id == model.Id)
                .Select(k => new { k.Aktiv, k.Intern })
                .SingleOrDefaultAsync();

            var tModel = _mapper.Map<TKunde>(model);
            context.Entry(tModel).State = EntityState.Modified;

            await context.SaveChangesAsync();

            // Der Kunde wird gerade deaktiviert, also ziehen Projekte und Aktivitäten nach.
            // Der interne Kunde ist ausgenommen: an ihm hängen die Ferien- und
            // Gleitzeit-Aktivitäten, die weiterhin bebuchbar bleiben müssen.
            if (bisher is { Aktiv: true, Intern: false } && !model.Aktiv)
            {
                await DeactivateProjekteAndVorgaengeAsync(context, model.Id);
            }
        }

        public async Task<List<Kunde>> DeleteAsync(Kunde model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var hasProjekte = await context.TProjekt.AnyAsync(p => p.KundeId == model.Id);

            if (hasProjekte)
            {
                var istIntern = await context.TKunde
                    .AsNoTracking()
                    .Where(k => k.Id == model.Id)
                    .Select(k => k.Intern)
                    .SingleOrDefaultAsync();

                model.Aktiv = false;
                var tModel = _mapper.Map<TKunde>(model);
                context.Entry(tModel).State = EntityState.Modified;
                await context.SaveChangesAsync();

                if (!istIntern)
                {
                    await DeactivateProjekteAndVorgaengeAsync(context, model.Id);
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

        private static async Task DeactivateProjekteAndVorgaengeAsync(ZeiterfassungContext context, Guid kundeId)
        {
            var projektIds = await context.TProjekt
                .Where(p => p.KundeId == kundeId)
                .Select(p => p.Id)
                .ToListAsync();

            if (projektIds.Count == 0)
            {
                return;
            }

            await context.TProjekt
                .Where(p => projektIds.Contains(p.Id) && p.Aktiv)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Aktiv, false));

            await context.TVorgang
                .Where(v => projektIds.Contains(v.ProjektId) && v.Aktiv)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Aktiv, false));
        }
    }
}
