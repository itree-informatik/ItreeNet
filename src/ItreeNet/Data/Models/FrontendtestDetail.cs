namespace ItreeNet.Data.Models
{
    public class FrontendtestDetail
    {
        public Guid Id { get; set; }
        public Guid FrontendtestId { get; set; }
        public string TestName { get; set; } = null!;
        public string Dauer { get; set; } = null!;
        public string Ergebnis { get; set; } = null!;
        public string? StdOut { get; set; }
        public string? ErrMessage { get; set; }
        public string? StackTrace { get; set; }

        public List<FrontendtestBild> FrontendtestBildListe { get; set; } = [];
    }
}
