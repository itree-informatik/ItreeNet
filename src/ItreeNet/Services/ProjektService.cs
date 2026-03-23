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

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TProjekt>(model);
                context.TProjekt.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TProjekt>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return await GetAllAsync();
        }

        public async Task<Projekt> SaveSingleAsync(Projekt model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();

                var tModel = _mapper.Map<TProjekt>(model);
                context.TProjekt.Add(tModel);
            }
            else
            {
                var tModel = _mapper.Map<TProjekt>(model);
                context.Entry(tModel).State = EntityState.Modified;
            }

            await context.SaveChangesAsync();

            return model;
        }

        public async Task<List<Projekt>> DeleteAsync(Projekt model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var anyVorhaben = await context.TVorgang.Where(v => v.ProjektId == model.Id).ToListAsync();

            if (anyVorhaben.Any())
            {
                model.Aktiv = false;
                var tModel = _mapper.Map<TProjekt>(model);
                context.Entry(tModel).State = EntityState.Modified;

                var aktiveVorhaben = anyVorhaben.Where(v => v.Aktiv).ToList();

                foreach (var vorhaben in aktiveVorhaben)
                {
                    vorhaben.Aktiv = false;
                    context.Entry(vorhaben).State = EntityState.Modified;
                }
            }
            else
            {
                var tModel = _mapper.Map<TProjekt>(model);
                context.TProjekt.Remove(tModel);
            }

            await context.SaveChangesAsync();

            return await ProjectsByCustomerIdAsync(model.KundeId, EnumStatus.Alle);
        }
    }
}
