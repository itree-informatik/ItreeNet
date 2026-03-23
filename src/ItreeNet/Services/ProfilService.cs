using AutoMapper;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ItreeNet.Services
{
    public class ProfilService : IProfilService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;

        public ProfilService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
        }

        public async Task UpdateAsync(Profil item, ProfilEinstellungen settings)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tItem = _mapper.Map<TProfil>(item);
            tItem.Wert = JsonSerializer.Serialize(settings);
            context.Update(tItem);

            await context.SaveChangesAsync();
        }
    }
}
