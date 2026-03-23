namespace ItreeNet.Data.Models
{
    public class ArbeitszeitReduktion
    {
        public Guid Id { get; set; }
        public DateOnly Datum { get; set; }
        public decimal Reduktion { get; set; }
    }
}
