using GemBox.Document;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using GemBox.Document.Tables;
using ItreeNet.Data.Extensions;
#pragma warning disable CS8601
#pragma warning disable CS8620
#pragma warning disable CS8602
#pragma warning disable CS8604

namespace ItreeNet.Services
{
    public class DokumentService: IDokumentService
    {
        private readonly ZeiterfassungContext _context;
        private readonly UserService _userService;
        private readonly string _tempVerzeichnis;
        private readonly string _vorlagenVerzeichnis;
        private DocumentModel _dokument = null!;

        public DokumentService(ZeiterfassungContext context, UserService userService, IConfiguration config)
        {
            _context = context;
            _userService = userService;
            var serialKey = config["LicenseKeys:GEMBOX_DOCUMENT_KEY"]?.Trim();
            ComponentInfo.SetLicense(serialKey);
            _tempVerzeichnis = $"{Globals.FileStorePath}/Temp/{_userService.CurrentUser!.MitarbeiterId!.Value}/";
            _vorlagenVerzeichnis = $"{Globals.FileStorePath}/Vorlagen/";
        }

        /// <summary>
        /// Arbeitsrapport erstellen für alle Kunden und Projekte auf die gebucht wurde in der angegebenen
        /// Periode. Pro Projekt wird eine Datei erstellt und am Schluss werden alle Dateien in ein gezipt
        /// </summary>
        /// <param name="jahr"></param>
        /// <param name="monatVon"></param>
        /// <param name="monatBis"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> CreateArbeitsrapporteOffeneBuchungen(DateOnly bis)
        {
            ClearTempFolder();
            var liste = await GetOffeneBuchungen(bis);
            return await CreateArbeitsrapporteFromList(liste);
        }

        public async Task<string> CreateArbeitsrapporte(int jahr, int monatVon, int monatBis)
        {
            ClearTempFolder();

            var von = new DateOnly(jahr, monatVon, 1);
            var bis = new DateOnly(jahr, monatBis, 1).AddMonths(1).AddDays(-1);
            var liste = await GetBuchungen(von, bis);
            return await CreateArbeitsrapporteFromList(liste);
        }

        private async Task<string> CreateArbeitsrapporteFromList(IList<ReportBuchung> liste)
        {
            if (!liste.Any())
            {
                throw new Exception("Keine Daten gefunden");
            }

            var vorlage = $"{_vorlagenVerzeichnis}Arbeitsrapport.dotx";
            if (!File.Exists(vorlage))
            {
                throw new Exception($"Dokumentvorlage nicht gefunden: {vorlage}");
            }
            var tempVerzeichnis = $"{_tempVerzeichnis}{Guid.NewGuid()}/";
            Directory.CreateDirectory(tempVerzeichnis);

            var def = new List<object[]>
            {
                new object[] { "H1", 0, null! },
                new object[] { "H2", 1, null! },
                new object[] { "D", 2, null! },
                new object[] { "T1", 3, null! },
                new object[] { "T2", 4, null! }
            };

            var minDatum = liste.Min(b => b.BuchungDatum);
            var maxDatum = liste.Max(b => b.BuchungDatum);
            var reportDatum = minDatum.Month != maxDatum.Month
                ? $"{minDatum:MMMM} - {maxDatum:MMMM} {minDatum:yyyy}"
                : $"{minDatum:MMMM} {minDatum:yyyy}";


            var proId = liste[0].ProjektId;
            var mitId = liste[0].MitarbeiterId;
            var dictonary = FillDictionary(liste[0].KundeName, liste[0].MitarbeiterName, reportDatum,
                liste[0].ProjektNummer,
                liste[0].ProjektBezeichnung);
            var totalListe = FillTotalListe(liste.Where(b => b.ProjektId == proId && b.MitarbeiterId == mitId)
                .OrderBy(o => o.VorgangBezeichnung).ToList());
            var detailListe = new List<string[]>
            {
                new[] { "H1" },
                new[] { "H2" }
            };
            var datum = DateOnly.FromDateTime(DateTime.MinValue);
            var dokumentname =
                $"{tempVerzeichnis}{liste[0].KundeName}_{liste[0].ProjektNummer.Replace("/", "-")}_{liste[0].MitarbeiterName.Replace(" ", string.Empty)}_{reportDatum.Replace(" ", string.Empty)}.pdf";
            var zusammenzugliste = liste.Where(b => b.ProjektId == proId).OrderBy(o => o.VorgangBezeichnung)
                .ToList();
            var totalMinuten = zusammenzugliste.Where(v => v.BuchungZeit.HasValue && v.ProjektId == proId && v.MitarbeiterId == mitId).Sum(v => v.BuchungZeit!.Value);

            foreach (var buc in liste)
            {
                if (buc.ProjektId != proId || buc.MitarbeiterId != mitId)
                {
                    detailListe.Add(new[] { "T1" });
                    detailListe.Add(new[] { "T2", null!, totalMinuten.FormatMinutenAlsZeit() });
                    CreateDocument(dictonary, def, detailListe, def, totalListe, vorlage, dokumentname);
                    proId = buc.ProjektId;
                    mitId = buc.MitarbeiterId;
                    dictonary = FillDictionary(buc.KundeName, buc.MitarbeiterName, reportDatum, buc.ProjektNummer,
                        buc.ProjektBezeichnung);
                    zusammenzugliste = liste.Where(b => b.ProjektId == proId && b.MitarbeiterId == mitId)
                        .OrderBy(o => o.VorgangBezeichnung)
                        .ToList();
                    totalMinuten = zusammenzugliste.Where(v => v.BuchungZeit.HasValue).Sum(v => v.BuchungZeit!.Value);
                    totalListe = FillTotalListe(zusammenzugliste);
                    datum = DateOnly.FromDateTime(DateTime.MinValue);
                    dokumentname =
                        $"{tempVerzeichnis}{buc.KundeName}_{buc.ProjektNummer.Replace("/", "-")}_{buc.MitarbeiterName.Replace(" ", string.Empty)}_{reportDatum.Replace(" ", string.Empty)}.pdf";
                    detailListe.Clear();
                    detailListe.Add(new[] { "H1" });
                    detailListe.Add(new[] { "H2" });
                }

                detailListe.Add(new[]
                {
                    "D",
                    buc.BuchungDatum == datum ? string.Empty : buc.BuchungDatum.ToString("dd.MM.yyyy"),
                    buc.BuchungVon.HasValue ? buc.BuchungVon.Value.ToString("HH:mm") : string.Empty,
                    buc.BuchungBis.HasValue ? buc.BuchungBis.Value.ToString("HH:mm") : string.Empty,
                    buc.VorgangBezeichnung,
                    buc.BuchungText,
                    buc.BuchungZeit.HasValue ? buc.BuchungZeit.Value.FormatMinutenAlsZeit() : string.Empty
                });
                datum = buc.BuchungDatum;
            }

            detailListe.Add(new[] { "T1" });
            detailListe.Add(new[] { "T2", null!, totalMinuten.FormatMinutenAlsZeit() });
            CreateDocument(dictonary, def, detailListe, def, totalListe, vorlage, dokumentname);

            // Dokumente in zip für Download
            var files = Directory.GetFiles(tempVerzeichnis);
            var zipFile = $"{_tempVerzeichnis}Arbeitsrapporte {reportDatum}.zip";
            if (File.Exists(zipFile))
            {
                File.Delete(zipFile);
            }
            using (var archiv = ZipFile.Open(zipFile, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    archiv.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }
            Directory.Delete(tempVerzeichnis, true);
            return zipFile;
        }

        /// <summary>
        /// Spesenabrechnung für einen Mitarbeiter erstellen. Es werden alle Spesenbuchungen
        /// berücksichtigt, die noch kein Einreichungsdatum haben.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> CreateSpesenabrechnung()
        {
            ClearTempFolder();

            var spesenListe = await _context.TSpesen
                .AsNoTracking()
                .Where(s => s.EingereichtAm == null && s.MitarbeiterId == _userService.CurrentUser!.MitarbeiterId!.Value &&
                            s.Datum <= DateOnly.FromDateTime(DateTime.Today.Date))
                .Join(_context.TMitarbeiter, spe => spe.MitarbeiterId, mit => mit.Id,
                    (spe, mit) => new
                    {
                        spe,
                        mit
                    })
                .OrderBy(o => o.spe.Datum)
                .ThenByDescending(o => o.spe.Betrag)
                .Select(n => new ReportSpesen
                {
                    MitarbeiterId = n.mit.Id,
                    MitarbeiterName = $"{n.mit.Nachname} {n.mit.Vorname}",
                    Datum = n.spe.Datum,
                    Betrag = n.spe.Betrag,
                    AnlassOrt = n.spe.AnlassOrt,
                    Spesenart = n.spe.Spesenart
                })
                .ToListAsync();

            if (!spesenListe.Any())
            {
                throw new Exception("Keine Daten gefunden");
            }


            var vorlage = $"{_vorlagenVerzeichnis}Spesenabrechnung.dotx";
            if (!Load(vorlage))
            {
                throw new Exception($"Dokumentvorlage nicht gefunden: {vorlage}");
            }

            if (!Directory.Exists(_tempVerzeichnis))
            {
                Directory.CreateDirectory(_tempVerzeichnis);
            }

            var def = new List<object[]>
            {
                new object[] { "H", 0, null! },
                new object[] { "D", 1, null! },
                new object[] { "T1", 2, null! },
                new object[] { "T2", 3, null! },
                new object[] { "T3", 4, null! },
                new object[] { "T4", 5, null! },
                new object[] { "T5", 6, null! }
            };

            var minDatum = spesenListe.Min(s => s.Datum);
            var maxDatum = spesenListe.Max(s => s.Datum);
            var reportDatum = minDatum.Month != maxDatum.Month
                ? $"{minDatum:MMMM} - {maxDatum:MMMM} {minDatum:yyyy}"
                : $"{minDatum:MMMM} {minDatum:yyyy}";

            var pdfFile = $"{_tempVerzeichnis}Spesenabrechnung {spesenListe[0].MitarbeiterName} {reportDatum}.pdf";
            if (File.Exists(pdfFile))
            {
                File.Delete(pdfFile);
            }

            var total = spesenListe.Sum(t => t.Betrag);

            var dictonary = new Dictionary<string, string>
            {
                { "[%Mitarbeiter%]", spesenListe[0].MitarbeiterName },
                { "[%Datum%]", reportDatum }
            };

            var liste = new List<string[]>
            {
                new[] { "H" }
            };
            liste.AddRange(spesenListe.Select(spesen => new[]
            {
                "D", spesen.AnlassOrt, spesen.Datum.ToString("dd.MM.yyyy"), spesen.Spesenart,
                spesen.Betrag.ToString("N2")
            }));
            liste.Add(new[] { "T1" });
            liste.Add(new[] { "T2", null!, total.ToString("N2") });
            liste.Add(new[] { "T3" });
            liste.Add(new[] { "T4" });
            liste.Add(new[] { "T5" });

            InsertValues(dictonary);
            FillTable("Detail", def, liste);
            Save(pdfFile);

            // Einreichdatum setzen
            var tSpe = await _context.TSpesen
                .Where(s => s.EingereichtAm == null && s.MitarbeiterId == _userService.CurrentUser!.MitarbeiterId!.Value &&
                            s.Datum <= DateOnly.FromDateTime(DateTime.Today))
                .ToListAsync();
            foreach (var spesen in tSpe)
            {
                spesen.EingereichtAm = DateOnly.FromDateTime(DateTime.Today);
                _context.Entry(spesen).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();

            return pdfFile;
        }

        /// <summary>
        /// Projektübersicht für eine Periode erstellen.
        /// </summary>
        /// <param name="jahr"></param>
        /// <param name="monatVon"></param>
        /// <param name="monatBis"></param>
        /// <param name="projektId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> CreateProjektUebersicht(int jahr, int monatVon, int monatBis, Guid projektId)
        {
            ClearTempFolder();

            var von = new DateOnly(jahr, monatVon, 1);
            var bis = new DateOnly(jahr, monatBis, 1).AddMonths(1).AddDays(-1);
            var liste = await GetBuchungen(von, bis, projektId);
            if (!liste.Any())
            {
                throw new Exception("Keine Daten gefunden");
            }

            liste = liste.OrderBy(l => l.ProjektNummer).ThenBy(l => l.VorgangBezeichnung).ThenBy(l => l.MitarbeiterName)
                .ToList();
            var vorlage = $"{_vorlagenVerzeichnis}Projektuebersicht.dotx";
            if (!Load(vorlage))
            {
                throw new Exception($"Dokumentvorlage nicht gefunden: {vorlage}");
            }

            var section = _dokument.Sections.First();
            var absaetze = section.Blocks.OfType<Paragraph>().ToList();
            var absatz = absaetze[0].Clone(true);
            var ma = absaetze[1].Clone(true);
            var tabellen = section.Blocks.OfType<Table>().ToList();
            var header = tabellen[0].Clone(true);
            var buchungen = tabellen[1].Clone(true);
            var totalVorgang = tabellen[2].Clone(true);
            var totalProjekt = tabellen[3].Clone(true);
            section.Blocks.Clear();

            var def = new List<object[]>
            {
                new object[] { "H1", 0, null! },
                new object[] { "H2", 1, null! },
                new object[] { "D", 2, null! },
                new object[] { "T1", 3, null! },
                new object[] { "T2", 4, null! }
            };

            var minDatum = liste.Min(b => b.BuchungDatum);
            var maxDatum = liste.Max(b => b.BuchungDatum);
            var reportDatum = minDatum.Month != maxDatum.Month
                ? $"{minDatum:MMMM} - {maxDatum:MMMM} {minDatum:yyyy}"
                : $"{minDatum:MMMM} {minDatum:yyyy}";

            var dictonary = new Dictionary<string, string>
            {
                { "[%Kunde%]", liste[0].KundeName },
                { "[%ProjektNummer%]", liste[0].ProjektNummer },
                { "[%ProjektBezeichnung%]", liste[0].ProjektBezeichnung },
                { "[%VorgangBezeichnung%]", liste[0].VorgangBezeichnung },
                { "[%Datum%]", reportDatum }
            };

            var dokumentname =
                $"{_tempVerzeichnis}{liste[0].KundeName}_{liste[0].ProjektNummer.Replace("/", "-")}_{reportDatum.Replace(" ", string.Empty)}.pdf";

            var vorId = liste[0].VorgangId;
            var vorBezeichnung = liste[0].VorgangBezeichnung;
            var mitId = liste[0].MitarbeiterId;
            var mitarbeiter = liste[0].MitarbeiterName;
            int totalMitMin = 0;
            int totalVorMin = 0;
            int totalProMin = 0;

            section.Blocks.Add(header.Clone(true));
            section.Blocks.Add(absatz.Clone(true));

            InsertValues(dictonary);
            dictonary.Remove("[%Datum%]");
            var detailListe = new List<string[]>();

            foreach (var buchung in liste)
            {
                if (buchung.VorgangId != vorId || buchung.MitarbeiterId != mitId)
                {
                    section.Blocks.Add(ma.Clone(true));
                    InsertValues(new Dictionary<string, string>
                        { { "[%Mitarbeiter%]", mitarbeiter } });
                    section.Blocks.Add(buchungen.Clone(true));
                    section.Blocks.Add(absatz.Clone(true));
                    detailListe.Insert(0, new[] { "H2" });
                    detailListe.Insert(0, new[] { "H1" });
                    detailListe.Add(new[] { "T1" });
                    detailListe.Add(new[] { "T2", null!, totalMitMin.FormatMinutenAlsZeit() });
                    FillTable("Mitarbeiter", def, detailListe);
                    var table = _dokument.GetChildElements(true, ElementType.Table)
                        .Cast<Table>().FirstOrDefault(t => t.Metadata.Title == "Mitarbeiter");
                    if (table != null)
                    {
                        table.Metadata.Title = string.Empty;
                    }
                    totalMitMin = 0;
                    mitId = buchung.MitarbeiterId;
                    mitarbeiter = buchung.MitarbeiterName;
                    detailListe.Clear();

                    if (buchung.VorgangId != vorId)
                    {
                        section.Blocks.Add(totalVorgang.Clone(true));
                        InsertValues(new Dictionary<string, string>
                            { {"[%VorgangBezeichnung%]", vorBezeichnung }, { "[%Total%]", totalVorMin.FormatMinutenAlsZeit() } });
                        section.Blocks.Add(absatz.Clone(true));
                        totalVorMin = 0;
                        vorBezeichnung = buchung.VorgangBezeichnung;
                        vorId = buchung.VorgangId;

                        section.Blocks.Add(new Paragraph(_dokument,
                            new SpecialCharacter(_dokument,
                                SpecialCharacterType.PageBreak)));
                        section.Blocks.Add(header.Clone(true));
                        section.Blocks.Add(absatz.Clone(true));
                        dictonary["[%VorgangBezeichnung%]"] = vorBezeichnung;
                        InsertValues(dictonary);
                    }
                }

                detailListe.Add(new[]
                {
                    "D",
                    buchung.BuchungDatum.ToString("dd.MM.yyyy"),
                    buchung.BuchungVon.HasValue ? buchung.BuchungVon.Value.ToString("HH:mm") : string.Empty,
                    buchung.BuchungBis.HasValue ? buchung.BuchungBis.Value.ToString("HH:mm") : string.Empty,
                    buchung.BuchungText,
                    buchung.BuchungZeit.HasValue ? buchung.BuchungZeit.Value.FormatMinutenAlsZeit() : string.Empty
                });
                totalMitMin += buchung.BuchungZeit ?? 0;
                totalVorMin += buchung.BuchungZeit ?? 0;
                totalProMin += buchung.BuchungZeit ?? 0;
            }
            section.Blocks.Add(ma.Clone(true));
            InsertValues(new Dictionary<string, string> { { "[%Mitarbeiter%]", mitarbeiter } });
            section.Blocks.Add(buchungen.Clone(true));
            detailListe.Insert(0, new[] { "H2" });
            detailListe.Insert(0, new[] { "H1" });
            detailListe.Add(new[] { "T1" });
            detailListe.Add(new[] { "T2", null!, totalMitMin.FormatMinutenAlsZeit() });
            FillTable("Mitarbeiter", def, detailListe);

            section.Blocks.Add(absatz.Clone(true));
            section.Blocks.Add(totalVorgang.Clone(true));
            InsertValues(new Dictionary<string, string>
                { {"[%VorgangBezeichnung%]", vorBezeichnung }, { "[%Total%]", totalVorMin.FormatMinutenAlsZeit() } });

            section.Blocks.Add(absatz.Clone(true));
            section.Blocks.Add(totalProjekt.Clone(true));
            InsertValues(new Dictionary<string, string>
                { {"[%ProjektBezeichnung%]", liste[0].ProjektBezeichnung}, { "[%Total%]", totalProMin.FormatMinutenAlsZeit() } });

            Save(dokumentname);
            return dokumentname;
        }

        /// <summary>
        /// Lesen aller Buchungen einer bestimmten Periode. Entweder werden alle Projekte geliefert oder
        /// es kann auch auf ein bestimmtes Projekt eingeschränkt werden.
        /// </summary>
        /// <param name="von"></param>
        /// <param name="bis"></param>
        /// <param name="proId"></param>
        /// <returns></returns>
        private async Task<IList<ReportBuchung>> GetBuchungen(DateOnly von, DateOnly bis, Guid? proId = null)
        {
            var liste = await _context.TBuchung
                .AsNoTracking()
                .Where(b => b.Datum >= von && b.Datum <= bis && !b.Provisorisch)
                .Join(_context.TVorgang, buc => buc.VorgangId, vor => vor.Id,
                    (buc, vor) => new
                    {
                        buc,
                        vor
                    })
                .Join(_context.TProjekt, vor => vor.vor.ProjektId, pro => pro.Id,
                    (vor, pro) => new
                    {
                        vor.buc,
                        vor.vor,
                        pro
                    })
                .Join(_context.TKunde, pro => pro.pro.KundeId, kun => kun.Id,
                    (pro, kun) => new
                    {
                        pro.buc,
                        pro.vor,
                        pro.pro,
                        kun
                    })
                .Join(_context.TMitarbeiter, kun => kun.buc.MitarbeiterId, mit => mit.Id,
                    (kun, mit) => new
                    {
                        kun.buc,
                        kun.vor,
                        kun.pro,
                        kun.kun,
                        mit
                    })
                .OrderBy(x => x.kun.Kundenname)
                .ThenBy(x => x.mit.Nachname)
                .ThenBy(x => x.mit.Vorname)
                .ThenBy(x => x.pro.Nummer)
                .ThenBy(x => x.buc.Datum)
                .ThenBy(x => x.buc.ZeitVon)
                .Select(n => new ReportBuchung
                {
                    KundeName = n.kun.Kundenname,
                    MitarbeiterId = n.mit.Id,
                    MitarbeiterName = $"{n.mit.Nachname} {n.mit.Vorname}",
                    ProjektId = n.pro.Id,
                    ProjektNummer = n.pro.Nummer,
                    ProjektBezeichnung = n.pro.Bezeichnung,
                    VorgangId = n.vor.Id,
                    VorgangBezeichnung = n.vor.Bezeichnung,
                    BuchungDatum = n.buc.Datum,
                    BuchungVon = n.buc.ZeitVon,
                    BuchungBis = n.buc.ZeitBis,
                    BuchungText = n.buc.Buchungstext,
                    BuchungZeit = n.buc.Zeit
                })
                .ToListAsync();
            if (!liste.Any())
            {
                return new List<ReportBuchung>();
            }

            return proId == null ? liste : liste.Where(l => l.ProjektId == proId).ToList();
        }

        private async Task<IList<ReportBuchung>> GetOffeneBuchungen(DateOnly bis)
        {
            var liste = await _context.TBuchung
                .AsNoTracking()
                .Where(b => b.Abgerechnet == null && !b.Provisorisch && b.Datum <= bis)
                .Join(_context.TVorgang, buc => buc.VorgangId, vor => vor.Id,
                    (buc, vor) => new { buc, vor })
                .Join(_context.TProjekt, vor => vor.vor.ProjektId, pro => pro.Id,
                    (vor, pro) => new { vor.buc, vor.vor, pro })
                .Join(_context.TKunde, pro => pro.pro.KundeId, kun => kun.Id,
                    (pro, kun) => new { pro.buc, pro.vor, pro.pro, kun })
                .Join(_context.TMitarbeiter, kun => kun.buc.MitarbeiterId, mit => mit.Id,
                    (kun, mit) => new { kun.buc, kun.vor, kun.pro, kun.kun, mit })
                .OrderBy(x => x.kun.Kundenname)
                .ThenBy(x => x.mit.Nachname)
                .ThenBy(x => x.mit.Vorname)
                .ThenBy(x => x.pro.Nummer)
                .ThenBy(x => x.buc.Datum)
                .ThenBy(x => x.buc.ZeitVon)
                .Select(n => new ReportBuchung
                {
                    KundeName = n.kun.Kundenname,
                    MitarbeiterId = n.mit.Id,
                    MitarbeiterName = $"{n.mit.Nachname} {n.mit.Vorname}",
                    ProjektId = n.pro.Id,
                    ProjektNummer = n.pro.Nummer,
                    ProjektBezeichnung = n.pro.Bezeichnung,
                    VorgangId = n.vor.Id,
                    VorgangBezeichnung = n.vor.Bezeichnung,
                    BuchungDatum = n.buc.Datum,
                    BuchungVon = n.buc.ZeitVon,
                    BuchungBis = n.buc.ZeitBis,
                    BuchungText = n.buc.Buchungstext,
                    BuchungZeit = n.buc.Zeit
                })
                .ToListAsync();

            return liste;
        }

        /// <summary>
        /// Für den Arbeitsrapport die einzelnen Dokumente erstellen
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="definition1"></param>
        /// <param name="tabelle1"></param>
        /// <param name="definition2"></param>
        /// <param name="tabelle2"></param>
        /// <param name="vorlage"></param>
        /// <param name="dokument"></param>
        private void CreateDocument(Dictionary<string, string> dictionary, IList<object[]> definition1,
            IList<string[]> tabelle1, IList<object[]> definition2, IList<string[]> tabelle2, string vorlage,
            string dokument)
        {
            Load(vorlage);
            InsertValues(dictionary);
            FillTable("Detail", definition1, tabelle1);
            FillTable("Zusammenzug", definition2, tabelle2);
            Save(dokument);
        }

        /// <summary>
        /// Abfüllen der generellen Daten im Arbeitsrapport
        /// </summary>
        /// <param name="kunde"></param>
        /// <param name="mitarbeiter"></param>
        /// <param name="datum"></param>
        /// <param name="proNummer"></param>
        /// <param name="proBezeichnung"></param>
        /// <returns></returns>
        private Dictionary<string, string> FillDictionary(string kunde, string mitarbeiter, string datum,
            string proNummer, string proBezeichnung)
        {
            var dictonary = new Dictionary<string, string>
            {
                { "[%Kunde%]", kunde },
                { "[%Mitarbeiter%]", mitarbeiter },
                { "[%Datum%]", datum },
                { "[%ProjektNummer%]", proNummer },
                { "[%ProjektBezeichnung%]", proBezeichnung }
            };
            return dictonary;
        }

        /// <summary>
        /// Zusammenfassung für den Arbeitsrapport pro Projekt erstellen
        /// </summary>
        /// <param name="buchungsListe"></param>
        /// <returns></returns>
        private List<string[]> FillTotalListe(IList<ReportBuchung> buchungsListe)
        {
            var vorListe = buchungsListe
                .GroupBy(b => b.VorgangBezeichnung)
                .Select(l => new { VorgangName = l.Key, SummeMinuten = l.Sum(b => b.BuchungZeit ?? 0) })
                .ToList()
                .OrderBy(o => o.VorgangName);

            var result = vorListe.Select(v => new[] { "D", null!, v.VorgangName, v.SummeMinuten.FormatMinutenAlsZeit() }).ToList();
            var totalMinuten = buchungsListe.Where(v => v.BuchungZeit.HasValue).Sum(v => v.BuchungZeit!.Value);
            result.Insert(0, new[] { "H2" });
            result.Insert(0, new[] { "H1" });
            result.Add(new[] { "T1" });
            result.Add(new[] { "T2", null!, totalMinuten.FormatMinutenAlsZeit() });
            return result!;
        }

        private bool Load(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return false;
            if (!File.Exists(filename))
                return false;
            _dokument = DocumentModel.Load(filename);
            return true;
        }

        private void Save(string documentname)
        {
            _dokument.Save(documentname);
        }

        private void InsertValues(Dictionary<string, string> dictionary)
        {
            foreach (var entry in dictionary)
            {
                var placeholder = _dokument.Content.Find(entry.Key).FirstOrDefault();
                while (placeholder != null)
                {
                    placeholder.LoadText(entry.Value);
                    placeholder = _dokument.Content.Find(entry.Key).FirstOrDefault();
                }
            }

            var initial = _dokument.Content.Find("[%initial%]").FirstOrDefault();
            while (initial != null)
            {
                initial.LoadText(string.Empty);
                initial = _dokument.Content.Find("[%initial%]").FirstOrDefault();
            }

            var start = _dokument.Content.Find("[%none%]").FirstOrDefault();
            while (start != null)
            {
                var end = _dokument.Content.Find("[%none%]").Skip(1).FirstOrDefault();
                if (end != null)
                {
                    var range = new ContentRange(start.Start, end.End);
                    range.LoadText(string.Empty);
                }
                else
                {
                    // Zweites [%none%] fehlt !!! --> Loop abbrechen
                    break;
                }
                start = _dokument.Content.Find("[%none%]").FirstOrDefault();
            }
        }

        private void FillTable(string title, IList<object[]> definition, IList<string[]> liste)
        {
            // title entspricht dem Titel im Alternativtext der Tabelleneigenschaften
            var table = _dokument.GetChildElements(true, ElementType.Table)
                .Cast<Table>().FirstOrDefault(t => t.Metadata.Title == title);
            if (table == null)
            {
                throw new Exception($"Tabelle '{title}' in Dokument nicht gefunden");
            }

            // anzahl tabellen spalten müssen mit anzahl einträgen übereinstimmen

            var columnCountListe = 0;
            foreach (var entry in liste)
            {
                if (entry.Length > columnCountListe)
                {
                    // Typ muss von der Laenge abgezogen werden
                    columnCountListe = entry.Length - 1;
                }
            }

            foreach (var row in definition)
            {
                var rowString = row[1].ToString();

                if (!string.IsNullOrEmpty(rowString))
                    row[2] = table.Rows[int.Parse(rowString)].Clone(true);
            }
            while (table.Rows.Count > 0)
            {
                table.Rows.RemoveAt(0);
            }

            var newTable = table.Clone(true);

            foreach (var row in liste)
            {
                if (row[0] == "PB")
                {
                    if (table.Parent != null)
                    {
                        var index = _dokument.Sections.IndexOf(table.Parent);
                        var indexBlock = _dokument.Sections[index].Blocks.IndexOf(table);
                        var pageBreak = new SpecialCharacter(_dokument, SpecialCharacterType.PageBreak);
                        var paragraph = new Paragraph(_dokument, new Run(_dokument, string.Empty), pageBreak);
                        _dokument.Sections[index].Blocks.Insert(indexBlock + 1, paragraph);
                        _dokument.Sections[index].Blocks.Insert(indexBlock + 2, newTable);
                    }

                    table = newTable;
                    continue;
                }
                foreach (var obj in definition)
                {
                    if (obj[0].ToString() == row[0])
                    {
                        table.Rows.Add(((TableRow)obj[2]).Clone(true));
                        for (var counter = 1; counter < row.Length; counter++)
                        {
                            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                            if (row[counter] != null)
                            {
                                var par = table.Rows[^1].Cells[counter - 1].Blocks[0].Content.GetChildElements(ElementType.Paragraph).Cast<Paragraph>().FirstOrDefault();
                                par?.Content.LoadText(row[counter]);
                            }
                        }
                        break;
                    }
                }
            }
        }

        private void ClearTempFolder()
        {
            if (Directory.Exists(_tempVerzeichnis))
            {
                Directory.Delete(_tempVerzeichnis, true);
            }
            Directory.CreateDirectory(_tempVerzeichnis);
        }
    }
}
