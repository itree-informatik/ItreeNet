namespace ItreeNet.Data.Models
{
    public class Vorgang
    {
        public Guid Id { get; set; }
        public Guid ProjektId { get; set; }
        public string Bezeichnung { get; set; } = null!;
        public bool Aktiv { get; set; }
        public bool Ferien { get; set; }
        public bool Gleitzeit { get; set; }
        public decimal Stundenansatz { get; set; }
        public decimal AnzahlStunden { get; set; }
        public decimal GebuchteStunden { get; set; }
        public Guid? KundeId => Projekt?.KundeId;

        public Projekt? Projekt { get; set; }
        public IList<Buchung>? Buchungen { get; set; }
    }
}
