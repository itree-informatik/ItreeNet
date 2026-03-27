using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services;

public class ApplikationService(IDbContextFactory<ZeiterfassungContext> dbFactory) : IApplikationService
{
    private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory = dbFactory;

    public async Task<List<Applikation>> GetAllAsync()
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        return await context.TApplikation
            .OrderBy(a => a.Bezeichnung)
            .Select(a => new Applikation
            {
                Id = a.Id,
                Bezeichnung = a.Bezeichnung
            })
            .ToListAsync();
    }

    public async Task<List<Applikation>> SaveAsync(Applikation app)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var existing = await context.TApplikation.FindAsync(app.Id);

        if (existing is null)
        {
            context.TApplikation.Add(new TApplikation
            {
                Id = app.Id == Guid.Empty ? Guid.NewGuid() : app.Id,
                Bezeichnung = app.Bezeichnung
            });
        }
        else
        {
            existing.Bezeichnung = app.Bezeichnung;
        }

        await context.SaveChangesAsync();
        return await GetAllAsync();
    }

    public async Task<List<Applikation>> DeleteAsync(Applikation app)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var existing = await context.TApplikation.FindAsync(app.Id);

        if (existing is null)
            return await GetAllAsync();

        context.TApplikation.Remove(existing);
        await context.SaveChangesAsync();
        return await GetAllAsync();
    }
}
