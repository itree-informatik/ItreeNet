namespace ItreeNet.Data.Models
{
    public class ProvisorischeBuchung
    {
        public Guid Id { get; set; }
        public DateOnly Datum { get; set; }
        public int? Zeit { get; set; }
        public string Buchungstext { get; set; } = string.Empty;
        public string? ProjektName { get; set; }
        public string? VorgangName { get; set; }
        public string? MitarbeiterName { get; set; }
    }
}
