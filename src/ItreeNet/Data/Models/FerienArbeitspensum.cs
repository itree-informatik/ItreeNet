namespace ItreeNet.Data.Models
{
    public class FerienArbeitspensum
    {
        public Guid Id { get; set; }
        public Guid MitarbeiterId { get; set; }
        public decimal FerienProJahr { get; set; } = 25m;
        public decimal Arbeitspensum { get; set; } = 100m;
        public DateOnly GueltigAb { get; set; }
        public bool Montag { get; set; } = true;
        public bool Dienstag { get; set; } = true;
        public bool Mittwoch { get; set; } = true;
        public bool Donnerstag { get; set; } = true;
        public bool Freitag { get; set; } = true;

        public Mitarbeiter? Mitarbeiter { get; set; }
    }
}
