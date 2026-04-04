namespace ItreeNet.Data.Models
{
    public class AnwesenheitZusammenfassung
    {
        public string Mitarbeiter { get; set; } = "";
        public decimal Anwesenheit { get; set; }
        public decimal Ferien { get; set; }
        public decimal Gleitzeit { get; set; }
        public decimal Krank { get; set; }
        public decimal Abwesenheit { get; set; }
        public decimal Gesamt => Anwesenheit + Ferien + Gleitzeit + Krank + Abwesenheit;
    }
}
