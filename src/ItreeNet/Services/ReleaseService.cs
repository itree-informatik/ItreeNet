using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Services;

public class ReleaseService(IDbContextFactory<ZeiterfassungContext> dbFactory) : IReleaseService
{
    private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory = dbFactory;

    public async Task<List<Release>> GetAllAsync(DateOnly? von = null, DateOnly? bis = null)
    {
        var heute = DateOnly.FromDateTime(DateTime.Today);
        var vonDatum = von ?? heute;
        var bisDatum = bis ?? new DateOnly(heute.Year, 12, 31);

        await using var context = await _dbFactory.CreateDbContextAsync();

        return await context.TRelease
            .Include(r => r.Applikation)
            .Where(r => r.Datum >= vonDatum && r.Datum <= bisDatum)
            .OrderBy(r => r.Datum)
            .ThenBy(r => r.Applikation.Bezeichnung)
            .Select(r => new Release
            {
                Id           = r.Id,
                ApplikationId = r.ApplikationId,
                Datum        = r.Datum,
                Bezeichnung  = r.Bezeichnung,
                ApplikationName = r.Applikation.Bezeichnung
            })
            .ToListAsync();
    }

    public async Task<List<Release>> GetNextPerAppAsync()
    {
        var heute = DateOnly.FromDateTime(DateTime.Today);
        await using var context = await _dbFactory.CreateDbContextAsync();

        var nextDates = context.TRelease
            .Where(r => r.Datum >= heute)
            .GroupBy(r => r.ApplikationId)
            .Select(g => new { ApplikationId = g.Key, MinDatum = g.Min(r => r.Datum) });

        return await context.TRelease
            .Join(nextDates,
                r => new { r.ApplikationId, r.Datum },
                n => new { n.ApplikationId, Datum = n.MinDatum },
                (r, _) => r)
            .Select(r => new Release
            {
                Id = r.Id,
                ApplikationId = r.ApplikationId,
                Datum = r.Datum,
                Bezeichnung = r.Bezeichnung,
                ApplikationName = r.Applikation.Bezeichnung
            })
            .OrderBy(r => r.Datum)
            .ToListAsync();
    }

    public async Task<List<Release>> SaveAsync(Release release)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var existing = await context.TRelease.FindAsync(release.Id);

        if (existing is null)
        {
            context.TRelease.Add(new TRelease
            {
                Id            = release.Id == Guid.Empty ? Guid.NewGuid() : release.Id,
                ApplikationId = release.ApplikationId,
                Datum         = release.Datum,
                Bezeichnung   = release.Bezeichnung
            });
        }
        else
        {
            existing.ApplikationId = release.ApplikationId;
            existing.Datum         = release.Datum;
            existing.Bezeichnung   = release.Bezeichnung;
        }

        await context.SaveChangesAsync();
        return await GetAllAsync();
    }

    public async Task<List<Release>> DeleteAsync(Release release)
    {
        await using var context = await _dbFactory.CreateDbContextAsync();

        var existing = await context.TRelease.FindAsync(release.Id);

        if (existing is null)
            return await GetAllAsync();

        context.TRelease.Remove(existing);
        await context.SaveChangesAsync();
        return await GetAllAsync();
    }
}
