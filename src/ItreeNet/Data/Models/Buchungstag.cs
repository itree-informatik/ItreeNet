using ItreeNet.Data.Models.DB;
using Microsoft.EntityFrameworkCore;

namespace ItreeNet.Data.Models
{
    public class Buchungstag
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;

        public Buchungstag(IDbContextFactory<ZeiterfassungContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public string? Weekday { get; set; } = null!;
        public DateOnly Date { get; set; }
        public int TotalMinutes
        {
            get
            {
                if (Details != null && Details.Any())
                {
                    return Details.Where(a => !a.Vorgang!.Gleitzeit).Sum(h => h.Zeit ?? 0);
                }
                return 0;
            }
        }
        /// <summary>
        /// TotalMinutes als Dezimalstunden (für Anwesenheit-Übertragung).
        /// </summary>
        public decimal TotalHours => TotalMinutes / 60m;
        public decimal? FeiertagReduktion
        {
            get
            {
                if (Date > new DateOnly(2000, 1, 1))
                {
                    using var context = _dbFactory.CreateDbContext();
                    var reduktion = context.TArbeitszeitReduktion.AsNoTracking().SingleOrDefault(r => r.Datum == Date);
                    if (reduktion != null)
                    {
                        return reduktion.Reduktion;
                    }
                }

                return null;
            }
        }
        public bool IsCollapsed { get; set; } = true;
        public List<Buchung>? Details { get; set; }
    }
}
