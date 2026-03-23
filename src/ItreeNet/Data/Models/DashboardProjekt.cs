namespace ItreeNet.Data.Models
{
    public class DashboardProjekt
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public Guid? KundenId { get; set; }
        public string? KundenName { get; set; }
        public string? Team { get; set; }
        public decimal? AnzahlStunden { get; set; }
        public decimal? GebuchteStunden { get; set; }
        public decimal Prozent
        {
            get
            {
                if(AnzahlStunden == null || AnzahlStunden == 0)
                    return Decimal.Zero;

                var prozent = GebuchteStunden * 100 / AnzahlStunden;
                if (prozent == null)
                    return Decimal.Zero;

                return Math.Round((decimal)prozent);
            }
        }
    }
}
