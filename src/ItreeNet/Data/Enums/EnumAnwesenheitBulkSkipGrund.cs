namespace ItreeNet.Data.Enums
{
    /// <summary>
    /// Grund, warum ein Tag bei der mehrtägigen Anwesenheitserfassung übersprungen wurde
    /// </summary>
    public enum EnumAnwesenheitBulkSkipGrund
    {
        Feiertag = 0,
        KeinArbeitstag = 1,
        BereitsVorhanden = 2
    }
}
