namespace ItreeNet.Data.Models
{
    public class FrontendtestOverview
    {
        public Guid Id { get; set; }
        public DateTime StartDatum { get; set; }
        public DateTime EndDatum { get; set; }
        public string Ergebnis { get; set; } = null!;
        public int Total { get; set; }
        public int Korrekt { get; set; }
        public int Fehlerhaft { get; set; }
    }
}
