namespace ItreeNet.Data.Models
{
    public class Frontendtest
    {
        public Guid Id { get; set; }
        public DateTime StartDatum { get; set; }
        public DateTime EndDatum { get; set; }
        public string Ergebnis { get; set; } = null!;
        public int Total { get; set; }
        public int Korrekt { get; set; }
        public int Fehlerhaft { get; set; }
        public string LastCommit { get; set; } = null!;

        public List<FrontendtestDetail> FrontendtestDetailListe { get; set; } = [];
    }
}
