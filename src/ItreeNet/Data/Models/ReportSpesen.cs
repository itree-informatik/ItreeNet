namespace ItreeNet.Data.Models
{
    public class ReportSpesen
    {
        public Guid MitarbeiterId { get; set; }
        public string? MitarbeiterName { get; set; }
        public DateOnly Datum { get; set; }
        public decimal Betrag { get; set; }
        public string? AnlassOrt { get; set; }
        public string? Spesenart { get; set; }
    }
}
