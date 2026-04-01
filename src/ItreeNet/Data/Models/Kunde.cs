namespace ItreeNet.Data.Models
{
    public class Kunde
    {
        public Guid Id { get; set; }
        public Guid? TeamId { get; set; }
        public string? Kundenname { get; set; }
        public bool Intern { get; set; }
        public bool Aktiv { get; set; } = true;
        public string? Adresse { get; set; }

        public Team? Team { get; set; }
    }
}
