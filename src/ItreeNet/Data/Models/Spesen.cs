namespace ItreeNet.Data.Models
{
    public class Spesen
    {
        public Guid Id { get; set; }
        public Guid MitarbeiterId { get; set; }
        public DateOnly Datum { get; set; }
        public decimal Betrag { get; set; }
        public string AnlassOrt { get; set; } = null!;
        public string Spesenart { get; set; } = null!;
        public bool Kreditkarte { get; set; }
        public DateOnly? EingereichtAm { get; set; }

        public Mitarbeiter? Mitarbeiter { get; set; }
    }
}
