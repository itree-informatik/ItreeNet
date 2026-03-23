namespace ItreeNet.Data.Models
{
    public class Arbeitszeit
    {
        public Guid Id { get; set; }
        public int Jahr { get; set; }
        public int Monat { get; set; }
        public decimal Zeit { get; set; }
        public decimal Tagesarbeitszeit { get; set; }
    }
}
