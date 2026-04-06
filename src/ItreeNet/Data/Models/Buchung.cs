namespace ItreeNet.Data.Models
{
    public class Buchung
    {
        public Guid Id { get; set; }
        public Guid VorgangId { get; set; }
        public Guid MitarbeiterId { get; set; }
        public DateOnly Datum { get; set; }
        public DateOnly? DatumBis { get; set; }
        public DateTime? ZeitVon { get; set; }
        public DateTime? ZeitBis { get; set; }
        public int? Zeit { get; set; }
        public string Buchungstext { get; set; } = string.Empty;
        public bool Stunden { get; set; }
        public DateTime? Abgerechnet { get; set; }
        public bool Provisorisch { get; set; }
        public Guid? ChangedBy { get; set; }
        public DateTime? ChangedOn { get; set; }
        public Guid? OriginalVorgangId { get; set; }
        public DateOnly? OriginalDatum { get; set; }
        public int? OriginalZeit { get; set; }
        public string? OriginalText { get; set; }

        public Mitarbeiter? ChangedByMitarbeiter { get; set; }
        public Mitarbeiter? Mitarbeiter { get; set; }
        public Vorgang? Vorgang { get; set; }
        public Vorgang? OriginalVorgang { get; set; }

        public string? KundenName { get; set; } 
        public string? ProjektName { get; set; }
    }
}
