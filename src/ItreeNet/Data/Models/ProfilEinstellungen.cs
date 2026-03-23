using ItreeNet.Data.Enums;

namespace ItreeNet.Data.Models
{
    public class ProfilEinstellungen
    {
        public string Mode { get; set; } = EnumThemeMode.Light.ToString();
        public List<ProfilTabellen>? Tabellen { get; set; }
    }

    public class ProfilTabellen
    {
        public string? Name { get; set; }
        public List<ProfilTabelleSpalten>? Spalten { get; set; }
    }

    public class ProfilTabelleSpalten
    {
        public string? Name { get; set; }
        public bool Show { get; set; }
    }
}
