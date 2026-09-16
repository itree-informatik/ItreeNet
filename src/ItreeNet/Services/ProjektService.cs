using AutoMapper;
using ItreeNet.Data.Enums;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services
{
    public class ProjektService : IProjektService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public ProjektService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task<List<Projekt>> GetAllAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tProjekte = await context.TProjekt
                .AsNoTracking()
                .Include(p => p.Kunde)
                .OrderBy(p => p.Bezeichnung)
                .ToListAsync();

            return _mapper.Map<List<Projekt>>(tProjekte);
        }

        public async Task<List<Projekt>> GetAllActiveAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tProjekte = await context.TProjekt
                .AsNoTracking()
                .Where(x => x.Aktiv == true)
                .ToListAsync();

            return _mapper.Map<List<Projekt>>(tProjekte);
        }

        public async Task<List<Projekt>> ProjectsByCustomerIdAsync(Guid? clientId, EnumStatus status = EnumStatus.Aktiv)
        {
            if (clientId == null)
            {
                return new List<Projekt>();
            }

            await using var context = await _dbFactory.CreateDbContextAsync();

            var tProjekte = await context.TProjekt
                .AsNoTracking()
                .Where(x => x.KundeId == clientId)
                .OrderBy(p => p.Bezeichnung)
                .ToListAsync();

            switch (status)
            {
                case EnumStatus.Aktiv:
                    tProjekte = tProjekte.Where(x => x.Aktiv).ToList();
                    break;
                case EnumStatus.Inaktiv:
                    tProjekte = tProjekte.Where(x => !x.Aktiv).ToList();
                    break;
            }

            return _mapper.Map<List<Projekt>>(tProjekte);
        }

        public async Task<List<Projekt>> AllProjectsByCustomerIdAsync(Guid? clientId)
        {
            if (clientId == null)
            {
                return new List<Projekt>();
            }

            await using var context = await _dbFactory.CreateDbContextAsync();

            var tProjekte = await context.TProjekt
                .AsNoTracking()
                .Where(x => x.KundeId == clientId)
                .OrderBy(p => p.Bezeichnung)
                .ToListAsync();

            return _mapper.Map<List<Projekt>>(tProjekte);
        }
        
        public async Task<List<Projekt>> SaveAsync(Projekt model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            await SaveWithCascadeAsync(context, model);

            return await GetAllAsync();
        }

        public async Task<Projekt> SaveSingleAsync(Projekt model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            await SaveWithCascadeAsync(context, model);

            return model;
        }

        private async Task SaveWithCascadeAsync(ZeiterfassungContext context, Projekt model)
        {
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tNeu = _mapper.Map<TProjekt>(model);
                context.TProjekt.Add(tNeu);

                await context.SaveChangesAsync();
                return;
            }

            var bisher = await context.TProjekt
                .AsNoTracking()
                .Where(p => p.Id == model.Id)
                .Select(p => new { p.Aktiv, KundeIntern = p.Kunde.Intern })
                .SingleOrDefaultAsync();

            var tModel = _mapper.Map<TProjekt>(model);
            context.Entry(tModel).State = EntityState.Modified;

            await context.SaveChangesAsync();

            // Das Projekt wird gerade deaktiviert, also ziehen die Aktivitäten nach. Projekte
            // des internen Kunden sind ausgenommen: an ihnen hängen die Ferien- und
            // Gleitzeit-Aktivitäten, die weiterhin bebuchbar bleiben müssen.
            if (bisher is { Aktiv: true, KundeIntern: false } && !model.Aktiv)
            {
                await DeactivateVorgaengeAsync(context, model.Id);
            }
        }

        public async Task<List<Projekt>> DeleteAsync(Projekt model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var hasVorhaben = await context.TVorgang.AnyAsync(v => v.ProjektId == model.Id);

            if (hasVorhaben)
            {
                var kundeIntern = await context.TProjekt
                    .AsNoTracking()
                    .Where(p => p.Id == model.Id)
                    .Select(p => p.Kunde.Intern)
                    .SingleOrDefaultAsync();

                model.Aktiv = false;
                var tModel = _mapper.Map<TProjekt>(model);
                context.Entry(tModel).State = EntityState.Modified;
                await context.SaveChangesAsync();

                if (!kundeIntern)
                {
                    await DeactivateVorgaengeAsync(context, model.Id);
                }
            }
            else
            {
                var tModel = _mapper.Map<TProjekt>(model);
                context.TProjekt.Remove(tModel);
                await context.SaveChangesAsync();
            }

            return await ProjectsByCustomerIdAsync(model.KundeId, EnumStatus.Alle);
        }

        private static async Task DeactivateVorgaengeAsync(ZeiterfassungContext context, Guid projektId)
        {
            await context.TVorgang
                .Where(v => v.ProjektId == projektId && v.Aktiv)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Aktiv, false));
        }
    }
}
