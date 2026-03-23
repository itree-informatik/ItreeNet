namespace ItreeNet.Data.Models
{
    public class FrontendtestBild
    {
        public Guid Id { get; set; }
        public Guid FrontendtestDetailId { get; set; }
        public string Verzeichnis { get; set; } = null!;
        public byte[] Bild { get; set; } = null!;

        public string BildBase64 => $"data:image/png;base64,{Convert.ToBase64String(Bild)}";
    }
}
