using ItreeNet.Data.Models;

namespace ItreeNet.Data.Extensions
{
    public static class BaseExtension
    {
        public static string FirstCharSubstring(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            if (input.ToLower() == "azureId")
            {
                return "AzureId";
            }

            return $"{input[0].ToString().ToUpper()}{input.Substring(1).ToLower()}";
        }

        /// <summary>
        /// Generiert neue Profileinstellungen. Muss erweitert werden, wenn weitere Einstellungen dazu kommen
        /// </summary>
        /// <returns></returns>
        public static ProfilEinstellungen GenereteProfilSettings()
        {
            var item = new ProfilEinstellungen()
            {
                Tabellen = GenerateProfilTabellen()
            };

            return item;
        }

        /// <summary>
        /// Generiert die ProfilTabellen. Muss erweitert werden, wenn weitere Tballen hinzukommen sollen
        /// </summary>
        /// <returns></returns>
        private static List<ProfilTabellen> GenerateProfilTabellen()
        {
            var list = new List<ProfilTabellen>()
            {
                new() {Name = "Taskboard", Spalten = GenerateProfilTabellenSpalten() }
            };

            return list;
        }

        /// <summary>
        /// Generiert alle Spalten zu der Tabelle Taskboard. Muss erweitert werden, wenn weitere Tabellen hinzukommen sollen
        /// </summary>
        /// <returns></returns>
        private static List<ProfilTabelleSpalten> GenerateProfilTabellenSpalten()
        {
            var list = new List<ProfilTabelleSpalten>()
            {
                new(){Name = "Prioritaet", Show = true},
                new(){Name = "Status", Show = true},
                new(){Name = "Projekt", Show = true},
                new(){Name = "Aktivitaet", Show = true},
                new(){Name = "Bezeichnung", Show = true},
                new(){Name = "Mitarbeiter", Show = true},
                new(){Name = "Kategorie", Show = true},
                new(){Name = "ChangedOn", Show = true},
            };

            return list;
        }
    }
}
