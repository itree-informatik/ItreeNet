namespace ItreeNet.Data.Models
{
    public class Team
    {
        public Guid Id { get; set; }
        public string? Bezeichnung { get; set; }
        public string? NaturalId { get; set; }
        public int? Sort { get; set; }
        public bool Aktiv { get; set; } = true;
    }
}
