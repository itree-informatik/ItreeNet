namespace ItreeNet.Data.Models
{
    public class MitarbeiterSaldo
    {
        public Guid Id { get; set; }
        public Guid MitarbeiterId { get; set; }
        public int Jahr { get; set; }
        public int Monat { get; set; }
        public decimal StundenSaldo { get; set; }
        public decimal FerienSaldo { get; set; }
        public decimal Soll { get; set; }
        public decimal Ist { get; set; }
    }
}
