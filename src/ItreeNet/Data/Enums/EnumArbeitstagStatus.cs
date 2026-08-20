namespace ItreeNet.Data.Enums
{
    /// <summary>
    /// Klassifikation eines Kalendertags für einen Mitarbeiter anhand von
    /// Arbeitspensum (Wochentag-Flags) und Feiertagen (T_ArbeitszeitReduktion)
    /// </summary>
    public enum EnumArbeitstagStatus
    {
        Arbeitstag = 0,
        Feiertag = 1,
        KeinArbeitstag = 2
    }
}
