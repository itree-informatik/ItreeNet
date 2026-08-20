namespace ItreeNet.Data.Models
{
    public class Anwesenheit
    {
        public Guid Id { get; set; }
        public Guid MitarbeiterId { get; set; }
        public DateOnly Datum { get; set; }
        // Nur für die mehrtägige Erfassung im UI — wird nicht persistiert (analog Buchung.DatumBis)
        public DateOnly? DatumBis { get; set; }
        public DateTime? ZeitVon { get; set; }
        public DateTime? ZeitBis { get; set; }
        public decimal? Zeit { get; set; }
        public string Typ { get; set; } = string.Empty;
        public string? Notiz { get; set; }

        public Mitarbeiter? Mitarbeiter { get; set; }
    }
}
