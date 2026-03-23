namespace ItreeNet.Data.Models
{
    public class DashboardMitarbeiter
    {
        public Guid Id { get; set; }
        public bool Intern { get; set; }
        public string? Fullname { get; set; }
        public decimal FerienSaldo { get; set; }
        public decimal StundenSaldo { get; set; }
        public bool BuchungAbgeschlossen { get; set; }
    }
}
