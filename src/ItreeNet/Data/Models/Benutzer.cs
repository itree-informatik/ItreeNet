using ItreeNet.Data.Enums;

namespace ItreeNet.Data.Models
{
    public class Benutzer
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        /// <summary>
        /// Azure User Id
        /// </summary>
        public Guid? Uid { get; set; }
        /// <summary>
        /// Database Mitarbeiter Id
        /// </summary>
        public Guid? MitarbeiterId { get; set; }
        public Profil? Profil { get; set; }
        public bool IsAuthenticated { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsAdmin { get; set; }
        public List<string>? Groups { get; set; }
        public bool IsMitarbeiter { get; set;}
        public bool IsIntern { get; set; }
        public EnumThemeMode? ThemeMode { get; set; }
    }
}
