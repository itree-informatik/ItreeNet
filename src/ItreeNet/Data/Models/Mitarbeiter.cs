namespace ItreeNet.Data.Models
{
    public class Mitarbeiter
    {
        public Guid Id { get; set; }
        public string? AzureId { get; set; }
        public string? Nachname { get; set; } 
        public string? Vorname { get; set; }
        public string? Email { get; set; }
        public DateOnly Eintritt { get; set; }
        public DateOnly? Austritt { get; set; }
        public bool Intern { get; set; } 
        public bool Aktiv { get; set; }

        public string Fullname => $"{Vorname} {Nachname}";
        
        public List<Guid>? TeamIds { get; set; }
        public IList<Spesen>? Spesen { get; set; }
        public IList<FerienArbeitspensum>? FerienArbeitspensum { get; set; }
    }
}
