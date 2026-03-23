namespace ItreeNet.Data.Models
{
    public class FerienArbeitspensum
    {
        public Guid Id { get; set; }
        public Guid MitarbeiterId { get; set; }
        public decimal FerienProJahr { get; set; }
        public decimal Arbeitspensum { get; set; }
        public DateOnly GueltigAb { get; set; }
        public bool Montag { get; set; }
        public bool Dienstag { get; set; }
        public bool Mittwoch { get; set; }
        public bool Donnerstag { get; set; }
        public bool Freitag { get; set; }

        public Mitarbeiter? Mitarbeiter { get; set; }
    }
}
