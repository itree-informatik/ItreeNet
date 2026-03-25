using ItreeMud.Services;

namespace ItreeNet.Data.Extensions;

public class ItreeNetTranslationProvider : ITranslationProvider
{
    private static readonly Dictionary<string, string> Translations = new()
    {
        { "BUTTON_SPEICHERN", "Speichern" },
        { "BUTTON_ABBRECHEN", "Abbrechen" },
        { "BUTTON_SCHLIESSEN", "Schliessen" },
        { "BUTTON_LOESCHEN", "Löschen" },
        { "BUTTON_HINZUFUEGEN", "Hinzufügen" },
        { "BUTTON_EDITIEREN", "Bearbeiten" },
        { "BUTTON_NEU", "Neu" },
        { "BUTTON_KOPIEREN", "Kopieren" },
        { "BUTTON_SUCHEN", "Suchen" },
        { "BUTTON_EXPORTIEREN", "Exportieren" },
        { "BUTTON_DOWNLOAD", "Download" },
        { "BUTTON_AKTUALISIEREN", "Aktualisieren" },
        { "BUTTON_AKTUALISIERENKLEIN", "Aktualisieren" },
        { "BUTTON_BERECHNEN", "Berechnen" },
        { "BUTTON_EINSTELLEN", "Einstellen" },
        { "BUTTON_EMAIL", "E-Mail" },
        { "BUTTON_ENTSPERREN", "Entsperren" },
        { "BUTTON_FREIGEBEN", "Freigeben" },
        { "BUTTON_JA", "Ja" },
        { "BUTTON_NEIN", "Nein" },
        { "BUTTON_NACHVERFOLGEN", "Nachverfolgen" },
        { "BUTTON_OEFFNEN", "Öffnen" },
        { "BUTTON_PLUS", "Plus" },
        { "BUTTON_REVIDIEREN", "Revidieren" },
        { "BUTTON_SPERREN", "Sperren" },
        { "BUTTON_TESTEN", "Testen" },
        { "BUTTON_VISIEREN", "Visieren" },
        { "BUTTON_WEITER", "Weiter" },
        { "BUTTON_ZURUECK", "Zurück" },
        { "BUTTON_ABSCHLIESSEN", "Abschliessen" },
        { "BUTTON_DOKUMENTEVERWALTEN", "Dokumente verwalten" },
        { "BUTTON_PROZESSEXKLUSION", "Prozess Exklusion" },
        { "BUTTON_PROZESSINTEGRATION", "Prozess Integration" },
    };

    public string Translate(string key)
    {
        if (Translations.TryGetValue(key, out var value))
            return value;

        // For keys like "Arbeitszeit_HINZUFUEGEN" → "Arbeitszeit hinzufügen"
        if (key.EndsWith("_HINZUFUEGEN"))
            return key.Replace("_HINZUFUEGEN", " hinzufügen");
        if (key.EndsWith("_BEARBEITEN"))
            return key.Replace("_BEARBEITEN", " bearbeiten");
        if (key.EndsWith("_ANZEIGEN"))
            return key.Replace("_ANZEIGEN", " anzeigen");

        return key;
    }
}
