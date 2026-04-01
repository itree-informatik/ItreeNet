namespace ItreeNet.Data.Models
{
    public class Projekt
    {
        public Guid Id { get; set; }
        public Guid KundeId { get; set; }
        public string? Nummer { get; set; }
        public string? Bezeichnung { get; set; }
        public bool Aktiv { get; set; } = true;
        public decimal Gesamtkosten { get; set; }
        public bool Pauschal { get; set; }
        public string? Vertragsdauer { get; set; }
        public bool Mehrwertsteuer { get; set; }
        public bool EmailGesendet80 { get; set; }
        public bool EmailGesendet90 { get; set; }
        public bool EmailGesendet100 { get; set; }

        public Kunde? Kunde { get; set; }
        public IList<Vorgang>? Vorgang { get; set; }
    }
}
