namespace ItreeNet.Interfaces;

public interface IDokumentService
{
    Task<string> CreateArbeitsrapporte(int jahr, int monatVon, int monatBis);
    Task<string> CreateArbeitsrapporteOffeneBuchungen();
    Task<string> CreateSpesenabrechnung();
    Task<string> CreateProjektUebersicht(int jahr, int monatVon, int monatBis, Guid projektId);
}