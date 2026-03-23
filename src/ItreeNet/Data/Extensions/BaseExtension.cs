using System.Text.Json;
using ItreeNet.Data.Enums;
using ItreeNet.Data.Models;
using ItreeNet.Shared.Form;

namespace ItreeNet.Data.Extensions
{
    public static class BaseExtension
    {
        /// <summary>
        ///     Copy a model to an other model by serialization and deserialization
        /// </summary>
        /// <typeparam name="T">Target model</typeparam>
        /// <param name="source">Source model</param>
        /// <returns></returns>
        public static T? Clone<T>(this T source)
        {
            var serialized = JsonSerializer.Serialize(source);
            return JsonSerializer.Deserialize<T>(serialized);
        }

        /// <summary>
        ///     Checks if two objets have the same properties and values
        /// </summary>
        /// <param name="a">Model to compare</param>
        /// <param name="b">Model to compare</param>
        /// <param name="ignoreProperties">Optional string array to ignore properties</param>
        /// <returns></returns>
        public static bool IsEqual(object? a, object? b, string? ignoreProperties = null)
        {
            if (a == null || b == null)
                return false;

            var aType = a.GetType();
            var ignorePropteriesList = new List<string>();
            if (ignoreProperties != null)
            {
                ignorePropteriesList = ignoreProperties.Split(',').ToList();
            }

            ignorePropteriesList.Add("ICollection`1");

            var ignoreList = aType.GetProperties().Where(p => p.Name.ToLower().Contains("translatedtext"));
            foreach (var ignore in ignoreList)
            {
                ignorePropteriesList.Add(ignore.Name);
            }

            foreach (var aProperty in aType.GetProperties())
            {
                if (aProperty.PropertyType.FullName != null && aProperty.PropertyType.IsClass && !aProperty.PropertyType.FullName.StartsWith("System"))
                    continue;

                if (!ignorePropteriesList.Contains(aProperty.PropertyType.Name) &&
                    !ignorePropteriesList.Contains(aProperty.Name))
                {
                    var aValue = aProperty.GetValue(a, null);
                    var bValue = aProperty.GetValue(b, null);

                    if (!Equals(aValue, bValue))
                    {
                        //Console.WriteLine($"Propertyname: {aProperty.Name}, a: {aValue} b:{bValue}");
                        return false;
                    }
                }
            }

            return true;
        }

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
                Label = dropdownLabel,
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
