using ItreeNet.Data.Models;
using ItreeMud.Models;

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
        /// Converts a list of objects to ItreeFormDropdownList
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">Object list</param>
        /// <param name="field">Field as string</param>
        /// <param name="propertyName">Property of Name</param>
        /// <param name="dropdownLabel">Label of dropdown</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public static ItreeFormDropdown CreateItreeFormDropdown<T>(IList<T> list, string field, string propertyName, string dropdownLabel)
        {
            var dropdown = new ItreeFormDropdown
            {
                Data = new List<ItreeFormDropdownItem>(),
                TranslationKey = dropdownLabel,
                Field = field,
            };

            foreach (var entry in list)
            {
                var dropdownItem = new ItreeFormDropdownItem();

                if (entry == null)
                    throw new InvalidCastException("Entry is null");

                dropdownItem.Text = entry.GetType().GetProperty(propertyName)?.GetValue(entry)?.ToString();
                var id = entry.GetType().GetProperty("Id")?.GetValue(entry)?.ToString();

                if (string.IsNullOrEmpty(id))
                    throw new InvalidCastException("Id not found");

                dropdownItem.Value = Guid.Parse(id);
                dropdown.Data.Add(dropdownItem);
            }
            return dropdown;
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
