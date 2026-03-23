using System.Text.Json;

namespace ItreeNet.Data.Models
{
    public class Profil
    {
        public Guid Id { get; set; }
        public Guid Mitarbeiterid { get; set; }
        public string? Wert { get; set; }

        public ProfilEinstellungen? Einstellungen
        {
            get
            {
                if(string.IsNullOrEmpty(Wert))
                    return null;

                return JsonSerializer.Deserialize<ProfilEinstellungen>(Wert);
            }
        }
    }
}
