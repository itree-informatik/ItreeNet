using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ItreeNet.Services
{
    public class PensumService : IPensumService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public PensumService(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<decimal> GetWeeklyWorkloadByEmployeeAsync(Guid id, DateOnly date)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var ferienArbeitspensum = await context.TFerienArbeitspensum
                                            .AsNoTracking()
                                            .Where(f => f.MitarbeiterId == id && f.GueltigAb <= date)
                                            .OrderByDescending(f => f.GueltigAb)
                                            .FirstOrDefaultAsync()
                                        ?? throw new InvalidDataException("FerienArbeitspensum not found");

            var arbeitszeit = await context.TArbeitszeit.AsNoTracking().SingleOrDefaultAsync(a => a.Jahr == date.Year && a.Monat == date.Month);
            if (arbeitszeit == null)
            {
                throw new InvalidDataException("Arbeitszeit not found");
            }

            var weeklyWorkload = (5 * arbeitszeit.Tagesarbeitszeit) * (ferienArbeitspensum.Arbeitspensum / 100);

            var weeklyWorkloadString = weeklyWorkload.ToString(CultureInfo.InvariantCulture);
            var weeklyWorkloadSubstring = weeklyWorkloadString.Split(".").Last();
            var digits = int.Parse(weeklyWorkloadSubstring);

            if (digits > 0)
            {
                return decimal.Round(weeklyWorkload, 2);
            }

            return decimal.Round(weeklyWorkload);
        }

        public async Task<decimal> GetDailyWorkloadByemployeeAsync(Guid id, DateOnly date)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var ferienArbeitspensum = await context.TFerienArbeitspensum
                                            .AsNoTracking()
                                            .Where(f => f.MitarbeiterId == id && f.GueltigAb <= date)
                                            .OrderByDescending(f => f.GueltigAb)
                                            .FirstOrDefaultAsync()
                                        ?? throw new InvalidDataException("FerienArbeitspensum not found");

            var anzahlProWoche = 0;
            if (ferienArbeitspensum.Montag)
            {
                anzahlProWoche++;
            }
            if (ferienArbeitspensum.Dienstag)
            {
                anzahlProWoche++;
            }
            if (ferienArbeitspensum.Mittwoch)
            {
                anzahlProWoche++;
            }
            if (ferienArbeitspensum.Donnerstag)
            {
                anzahlProWoche++;
            }
            if (ferienArbeitspensum.Freitag)
            {
                anzahlProWoche++;
            }

            var arbeitsZeit = await context.TArbeitszeit
                                                .AsNoTracking()
                                                .SingleOrDefaultAsync(a => a.Jahr == date.Year && a.Monat == date.Month)
                                            ?? throw new InvalidDataException($"Keine Arbeitszeit für {date.Month}/{date.Year} konfiguriert");

            var stundenProTag = ((arbeitsZeit.Tagesarbeitszeit) * 5) * (ferienArbeitspensum.Arbeitspensum / 100) /
                                anzahlProWoche;

            return stundenProTag;
        }

        public async Task<decimal> GetMonthlyWorkloadByEmployeeAsync(Guid id, DateOnly date)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var date1 = date;
            var workload = await context.TFerienArbeitspensum
                                                .AsNoTracking()
                                                .Where(x => x.MitarbeiterId == id && x.GueltigAb <= date1)
                                                .OrderByDescending(x => x.GueltigAb)
                                                .FirstOrDefaultAsync();

            if (workload == null)
            {
                throw new InvalidDataException("Employee not found");
            }

            var datumBis = new DateOnly(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
            var stundenProTag = await GetDailyWorkloadByemployeeAsync(id, date);

            var reduktionenDict = await context.TArbeitszeitReduktion
                .AsNoTracking()
                .Where(a => a.Datum >= date1 && a.Datum <= datumBis)
                .ToDictionaryAsync(a => a.Datum, a => a.Reduktion);

            decimal monthlyWorkload = 0;
            while (date <= datumBis)
            {
                reduktionenDict.TryGetValue(date, out var reduktion);

                var reduktionsValue = reduktion;

                if (reduktionsValue > 0 && workload.Arbeitspensum != 100 && workload.Montag && workload.Dienstag && workload.Mittwoch &&
                    workload.Donnerstag && workload.Freitag)
                {
                    reduktionsValue *= workload.Arbeitspensum / 100;
                }

                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        if (workload.Montag)
                        {
                            monthlyWorkload = monthlyWorkload + stundenProTag - reduktionsValue;
                        }
                        break;
                    case DayOfWeek.Tuesday:
                        if (workload.Dienstag)
                        {
                            monthlyWorkload = monthlyWorkload + stundenProTag - reduktionsValue;
                        }
                        break;
                    case DayOfWeek.Wednesday:
                        if (workload.Mittwoch)
                        {
                            monthlyWorkload = monthlyWorkload + stundenProTag - reduktionsValue;
                        }
                        break;
                    case DayOfWeek.Thursday:
                        if (workload.Donnerstag)
                        {
                            monthlyWorkload = monthlyWorkload + stundenProTag - reduktionsValue;
                        }
                        break;
                    case DayOfWeek.Friday:
                        if (workload.Freitag)
                        {
                            monthlyWorkload = monthlyWorkload + stundenProTag - reduktionsValue;
                        }
                        break;
                }

                date = date.AddDays(1);
            }
            
            return monthlyWorkload;
        }
    }
}
