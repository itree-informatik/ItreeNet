namespace ItreeNet.Data.Models
{
    public class ReportBuchung
    {
        public string? KundeName { get; set; }
        public Guid MitarbeiterId { get; set; }
        public string? MitarbeiterName { get; set; }
        public Guid ProjektId { get; set; }
        public string? ProjektNummer { get; set; }
        public string? ProjektBezeichnung { get; set; }
        public Guid VorgangId { get; set; }
        public string? VorgangBezeichnung { get; set; }
        public DateOnly BuchungDatum { get; set; }
        public DateTime? BuchungVon { get; set; }
        public DateTime? BuchungBis { get; set; }
        public string? BuchungText { get; set; }
        public int? BuchungZeit { get; set; }
    }
}
