namespace ItreeNet.Data.Models
{
    public class PerformanceList
    {
        public string? Name { get; set; }
        public List<Performance> Performances { get; set; } = new();
    }

    public class Performance
    {
        public string? Name { get; set; }
        public decimal Wert { get; set; }
        public decimal Prozent { get; set; }
        public decimal Total { get; set; }
        public string? Class { get; set; }
    }
}
