using ItreeNet.Data.Extensions;
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

        public ProfilService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task UpdateAsync(Profil item, ProfilEinstellungen settings)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tItem = item.ToEntity();
            tItem.Wert = JsonSerializer.Serialize(settings);
            context.Update(tItem);

            await context.SaveChangesAsync();
        }
    }
}
