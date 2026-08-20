using ItreeNet.Data.Enums;

namespace ItreeNet.Data.Models
{
    public class AnwesenheitBulkSkip
    {
        public DateOnly Datum { get; set; }
        public EnumAnwesenheitBulkSkipGrund Grund { get; set; }
    }

    /// <summary>
    /// Ergebnis der mehrtägigen Anwesenheitserfassung: wie viele Tage erstellt
    /// wurden und welche Tage aus welchem Grund übersprungen wurden
    /// </summary>
    public class AnwesenheitBulkResult
    {
        public int AnzahlErstellt { get; set; }
        public List<AnwesenheitBulkSkip> Uebersprungen { get; set; } = new();

        public int AnzahlUebersprungen => Uebersprungen.Count;
        public int AnzahlFeiertag => Uebersprungen.Count(s => s.Grund == EnumAnwesenheitBulkSkipGrund.Feiertag);
        public int AnzahlKeinArbeitstag => Uebersprungen.Count(s => s.Grund == EnumAnwesenheitBulkSkipGrund.KeinArbeitstag);
        public int AnzahlBereitsVorhanden => Uebersprungen.Count(s => s.Grund == EnumAnwesenheitBulkSkipGrund.BereitsVorhanden);
    }
}
