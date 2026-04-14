using ClosedXML.Excel;
using ItreeNet.Data.Models;
using Microsoft.JSInterop;

namespace ItreeNet.Pages;

public partial class Reports
{
    /// <summary>
    /// Lädt eine Datei vom Server-Dateisystem als Stream-Download herunter.
    /// </summary>
    private async Task DownloadFileAsync(string filePath)
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var streamRef = new DotNetStreamReference(stream: stream);
            await JsRuntime.InvokeVoidAsync("DownloadFileFromStream", Path.GetFileName(filePath), streamRef);
        }
    }

    /// <summary>
    /// Exportiert die aktuellen Buchungsergebnisse als Excel-Datei mit Summenzeile.
    /// </summary>
    private async Task ExportBuchungenExcelAsync()
    {
        if (_buchungen == null || _buchungen.Count == 0)
            return;

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Buchungen");

        ws.Cell(1, 1).Value = "Datum";
        ws.Cell(1, 2).Value = "Kunde";
        ws.Cell(1, 3).Value = "Projekt";
        ws.Cell(1, 4).Value = "Aktivität";
        ws.Cell(1, 5).Value = "Mitarbeiter";
        ws.Cell(1, 6).Value = "Zeit";
        ws.Cell(1, 7).Value = "Buchungstext";

        var headerRange = ws.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;

        for (var i = 0; i < _buchungen.Count; i++)
        {
            var b = _buchungen[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = b.Datum.ToString("dd.MM.yyyy");
            ws.Cell(row, 2).Value = b.KundenName;
            ws.Cell(row, 3).Value = b.ProjektName;
            ws.Cell(row, 4).Value = b.Vorgang?.Bezeichnung;
            ws.Cell(row, 5).Value = b.Mitarbeiter?.Fullname;
            ws.Cell(row, 6).Value = b.Zeit ?? 0;
            ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 7).Value = b.Buchungstext;
        }

        // Summenzeile
        var sumRow = _buchungen.Count + 2;
        ws.Cell(sumRow, 5).Value = "Total";
        ws.Cell(sumRow, 5).Style.Font.Bold = true;
        ws.Cell(sumRow, 6).Value = _buchungenZeitSum;
        ws.Cell(sumRow, 6).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(sumRow, 6).Style.Font.Bold = true;

        ws.Columns().AdjustToContents();

        await DownloadExcelAsBase64Async(workbook, $"Buchungen_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// Exportiert die Anwesenheitsdaten als Excel-Datei mit Detail- und Zusammenfassungsblatt.
    /// </summary>
    private async Task ExportAnwesenheitenExcelAsync()
    {
        if (_anwesenheiten == null || _anwesenheiten.Count == 0)
            return;

        using var workbook = new XLWorkbook();

        // Detailblatt
        var ws = workbook.Worksheets.Add("Anwesenheiten");
        ws.Cell(1, 1).Value = "Datum";
        ws.Cell(1, 2).Value = "Mitarbeiter";
        ws.Cell(1, 3).Value = "Typ";
        ws.Cell(1, 4).Value = "Stunden";
        ws.Cell(1, 5).Value = "Von";
        ws.Cell(1, 6).Value = "Bis";
        ws.Cell(1, 7).Value = "Notiz";

        var headerRange = ws.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;

        for (var i = 0; i < _anwesenheiten.Count; i++)
        {
            var a = _anwesenheiten[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = a.Datum.ToString("dd.MM.yyyy");
            ws.Cell(row, 2).Value = a.Mitarbeiter?.Fullname;
            ws.Cell(row, 3).Value = a.Typ;
            ws.Cell(row, 4).Value = a.Zeit ?? 0;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 5).Value = a.ZeitVon?.ToString("HH:mm") ?? "";
            ws.Cell(row, 6).Value = a.ZeitBis?.ToString("HH:mm") ?? "";
            ws.Cell(row, 7).Value = a.Notiz ?? "";
        }

        ws.Columns().AdjustToContents();

        // Zusammenfassungsblatt
        if (_anwesenheitenZusammenfassung.Count > 0)
        {
            var wsZ = workbook.Worksheets.Add("Zusammenfassung");
            wsZ.Cell(1, 1).Value = "Mitarbeiter";
            wsZ.Cell(1, 2).Value = "Anwesenheit";
            wsZ.Cell(1, 3).Value = "Ferien";
            wsZ.Cell(1, 4).Value = "Gleitzeit";
            wsZ.Cell(1, 5).Value = "Krank";
            wsZ.Cell(1, 6).Value = "Abwesenheit";
            wsZ.Cell(1, 7).Value = "Gesamt";

            var headerRangeZ = wsZ.Range(1, 1, 1, 7);
            headerRangeZ.Style.Font.Bold = true;

            for (var i = 0; i < _anwesenheitenZusammenfassung.Count; i++)
            {
                var z = _anwesenheitenZusammenfassung[i];
                var row = i + 2;
                wsZ.Cell(row, 1).Value = z.Mitarbeiter;
                wsZ.Cell(row, 2).Value = z.Anwesenheit;
                wsZ.Cell(row, 3).Value = z.Ferien;
                wsZ.Cell(row, 4).Value = z.Gleitzeit;
                wsZ.Cell(row, 5).Value = z.Krank;
                wsZ.Cell(row, 6).Value = z.Abwesenheit;
                wsZ.Cell(row, 7).Value = z.Gesamt;

                for (var col = 2; col <= 7; col++)
                    wsZ.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
            }

            wsZ.Columns().AdjustToContents();
        }

        await DownloadExcelAsBase64Async(workbook, $"Anwesenheitsauswertung_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// Konvertiert ein Excel-Workbook zu Base64 und löst den Browser-Download aus.
    /// </summary>
    private async Task DownloadExcelAsBase64Async(XLWorkbook workbook, string filename)
    {
        byte[] fileBytes;
        using (var stream = new MemoryStream())
        {
            workbook.SaveAs(stream);
            fileBytes = stream.ToArray();
        }

        var base64 = Convert.ToBase64String(fileBytes);
        await JsRuntime.InvokeVoidAsync("DownloadFileFromBase64", filename, base64);
    }
}
