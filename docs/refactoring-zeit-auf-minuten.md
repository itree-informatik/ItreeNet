# Refactoring: Zeit-Feld von Dezimal-Stunden auf Minuten (int) umstellen

## Hintergrund

Bestimmte Kunden verlangen eine minutengenaue Abrechnung statt der bisherigen Viertelstundenerfassung. Die Zeiterfassung soll so umgestellt werden, dass in der Datenbank Minuten als `int` gespeichert werden. Pro Kunde soll ein Abrechnungsintervall definierbar sein (z.B. 15 Minuten oder 1 Minute).

## Aktueller Stand

### Datentyp

| Tabelle | Spalte | Typ | Bedeutung |
|---|---|---|---|
| `TBuchung` | `Zeit` | `decimal(5,2)` | Stunden (z.B. 1.50 = 1h 30min) |
| `TBuchung` | `OriginalZeit` | `decimal` | Archivierte Originalzeit bei Korrektur |
| `TAnwesenheit` | `Zeit` | `decimal(5,2)` | Anwesenheitsstunden |
| `ReportBuchung` | `BuchungZeit` | `decimal?` | DTO fuer Berichtserstellung |
| `Buchungstag` | `TotalHours` | `decimal?` | Berechnete Tagessumme |

### Alle Nutzungsorte von `Zeit` / `BuchungZeit`

#### Models (Definitionen)

| Datei | Property | Typ |
|---|---|---|
| `Data/Models/DB/TBuchung.cs` | `Zeit` | `decimal?` mit `[Column(TypeName = "decimal(5, 2)")]` |
| `Data/Models/Buchung.cs` | `Zeit` | `decimal?` |
| `Data/Models/Buchung.cs` | `OriginalZeit` | `decimal?` |
| `Data/Models/DB/TAnwesenheit.cs` | `Zeit` | `decimal?` mit `[Column(TypeName = "decimal(5, 2)")]` |
| `Data/Models/Anwesenheit.cs` | `Zeit` | `decimal?` |
| `Data/Models/ReportBuchung.cs` | `BuchungZeit` | `decimal?` |
| `Data/Models/Buchungstag.cs` | `TotalHours` | Berechnet aus `Details.Sum(h => h.Zeit)` |

#### AutoMapper (Data/Extensions/Mappings.cs)

- `TBuchung` <-> `Buchung`: Direktes Mapping (kein Transform), `Zeit` wird 1:1 kopiert
- `ReportBuchung.BuchungZeit` wird manuell in `DokumentService` gesetzt: `BuchungZeit = n.buc.Zeit`

#### Services (Lesen/Schreiben/Berechnen)

**BuchungsService.cs:**
- `ChangedOn = DateTime.UtcNow` beim Speichern (kein direkter Zeit-Bezug, aber gleicher Save-Pfad)
- Saldo-Berechnung: `anwesenheiten.Sum(a => a.Zeit ?? 0m)` — Stundensumme fuer Monatssaldo
- Ferien-Berechnung: `ferienUsed / stundenProTag` — Stunden zu Tagen

**DashboardService.cs:**
- `buchungen.Sum(b => b.Zeit)` — Tagessumme, dann `buchungenSum / arbeitszeit` (Stunden zu Tagen)
- `tBuchungen.Sum(b => b.Zeit)` — Top-5-Buchungen, sortiert nach `OrderByDescending(b => b.Zeit)`
- `.Select(g => new { Stunden = g.Sum(b => b.Zeit) })` — Gruppierte Summe pro Vorgang
- `Zeit = b.Zeit` — Mapping auf Buchung-DTO fuer provisorische Buchungen

**VorgangService.cs:**
- `.SumAsync(b => b.Zeit)` — Gebuchte Stunden pro Vorgang
- `.Select(g => new { Stunden = g.Sum(b => b.Zeit) })` — Gruppierte Summe

**DokumentService.cs:**
- `BuchungZeit = n.buc.Zeit` — Mapping auf ReportBuchung (2x: GetBuchungen + GetOffeneBuchungen)
- `.Sum(v => v.BuchungZeit!.Value)` — Totale fuer Arbeitsrapporte (mehrfach)
- `.GroupBy().Select(l => new { Summe = l.Sum(b => b.BuchungZeit) })` — Zusammenfassung pro Vorgang
- Formatierung in Dokumenten: `$"{v.Summe:N2}"` — 2 Nachkommastellen
- Zwischensummen/Totale in Word-Dokumenten

#### UI-Seiten (Anzeige und Eingabe)

**ZeiterfassungTable.razor:**
- `@buchung.Zeit?.ToString("N2")` — Zellanzeige in Buchungstabelle
- `@daily.TotalHours?.ToString("N2")` — Tagessumme
- `@TimeBookingsTotalHours?.ToString("N2")` — Wochensumme mit "h"-Suffix
- `@AnwesenheitTotalHours.ToString("N2") h` — Anwesenheitssumme mit Farbcodierung
- `anwesenheit.Zeit = day.TotalHours` — Transfer Buchungsstunden in Anwesenheit
- `buchung.Zeit = 0` — Reset bei neuer Buchung

**TableWindow.razor (Buchungserfassung):**
- `<MudNumericField @bind-Value="@Buchung!.Zeit" Format="N2" />` — Direkte Stundeneingabe
- Berechnung aus Zeitspanne: `var hours = (zeitBis - zeitVon).TotalHours;` dann `Buchung.Zeit = Convert.ToDecimal(hours.ToString("N2"))`
- `Buchung.Zeit = null` — Reset bei Moduswechsel

**AnwesenheitWindow.razor:**
- `<MudNumericField @bind-Value="@_model.Zeit" Format="N2" Step="0.25M" />` — Eingabe in 0.25h-Schritten
- Berechnung: `(_model.ZeitBis - _model.ZeitVon).TotalHours - pause` -> `_model.Zeit`
- Pause-Berechnung: `grossHours - (double)_model.Zeit`

**Reports.razor:**
- `@item.Item.Zeit` — Zellanzeige im DataGrid
- `_buchungenZeitSum = _buchungen?.Sum(b => b.Zeit) ?? 0` — Fusszeilensumme
- `@_buchungenZeitSum h` — Anzeige

**Dashboard.razor:**
- `100 / _maxBuchungsStunden * buchung.Zeit` — Breite von Fortschrittsbalken (Prozent)
- `@buchung.Zeit h` — Tabellenanzeige

#### Validatoren

**BuchungValidator.cs:**
- `Zeit > 0` — Pflichtfeld
- `(zeit.Value * 100) % 25 != 0` — Viertelstundenpruefung
- Ferien-Sonderregel: `Zeit >= 4` (mindestens 4 Stunden)

**AnwesenheitValidator.cs:**
- `Zeit > 0` — Pflichtfeld
- `(zeit.Value * 100) % 25 != 0` — Viertelstundenpruefung
- Ferien-Sonderregel: `Zeit >= 4`

#### SQL-Migrationen

**17_Changes.sql:**
- `zeit NUMERIC NULL` — Spaltendefinition
- `originalzeit NUMERIC NULL` — Archiv-Spalte

---

## Geplantes Vorgehen

### Phase 1: SQL-Migration

Datenbank ist **PostgreSQL**. Konventionen: Schema `dbo`, Tabellennamen lowercase mit `t_`-Prefix
(z.B. `dbo.t_buchung`), Spaltennamen lowercase. Transaktionen mit `BEGIN;`/`COMMIT;`.
Siehe bestehende Scripte in `Data/Database/` (zuletzt `17_Changes.sql`).

```sql
-- 18_Changes.sql
-- Spalte "zeit" von NUMERIC (Dezimalstunden) auf INT (Minuten) umstellen.
-- Betrifft dbo.t_buchung (zeit, originalzeit) und dbo.t_anwesenheit (zeit).

-- Vorab pruefen ob es nicht-ganzzahlige Minutenwerte gibt:
-- SELECT id, zeit, zeit * 60 AS minuten FROM dbo.t_buchung WHERE (zeit * 60) % 1 != 0;
-- SELECT id, zeit, zeit * 60 AS minuten FROM dbo.t_anwesenheit WHERE (zeit * 60) % 1 != 0;

BEGIN;

-- 1. Neue Spalten hinzufuegen
ALTER TABLE dbo.t_buchung ADD COLUMN zeitminuten INT NULL;
ALTER TABLE dbo.t_buchung ADD COLUMN originalzeitminuten INT NULL;
ALTER TABLE dbo.t_anwesenheit ADD COLUMN zeitminuten INT NULL;

-- 2. Daten migrieren (Stunden * 60 = Minuten)
UPDATE dbo.t_buchung SET zeitminuten = ROUND(zeit * 60)::INT;
UPDATE dbo.t_buchung SET originalzeitminuten = ROUND(originalzeit * 60)::INT WHERE originalzeit IS NOT NULL;
UPDATE dbo.t_anwesenheit SET zeitminuten = ROUND(zeit * 60)::INT;

-- 3. Alte Spalten entfernen
ALTER TABLE dbo.t_buchung DROP COLUMN zeit;
ALTER TABLE dbo.t_buchung DROP COLUMN originalzeit;
ALTER TABLE dbo.t_anwesenheit DROP COLUMN zeit;

-- 4. Umbenennen
ALTER TABLE dbo.t_buchung RENAME COLUMN zeitminuten TO zeit;
ALTER TABLE dbo.t_buchung RENAME COLUMN originalzeitminuten TO originalzeit;
ALTER TABLE dbo.t_anwesenheit RENAME COLUMN zeitminuten TO zeit;

COMMIT;
```

### Phase 2: Models anpassen

**TBuchung.cs / TAnwesenheit.cs:**
```csharp
[Column(TypeName = "int")]
public int? Zeit { get; set; }  // Minuten
```

**Buchung.cs / Anwesenheit.cs (DTOs):**
```csharp
public int? Zeit { get; set; }  // Minuten

// Berechnete Property fuer die Anzeige
public decimal? Stunden => Zeit.HasValue ? Zeit.Value / 60m : null;
```

**ReportBuchung.cs:**
```csharp
public int? BuchungZeit { get; set; }  // Minuten
public decimal? BuchungStunden => BuchungZeit.HasValue ? BuchungZeit.Value / 60m : null;
```

**Buchungstag.cs:**
```csharp
// TotalHours-Berechnung anpassen:
// Alt:  Details.Sum(h => h.Zeit)
// Neu:  Details.Sum(h => h.Stunden)  // oder  Details.Sum(h => h.Zeit) / 60m
```

### Phase 3: Code anpassen (alle Nutzungsorte)

#### Berechnungen aus Zeitspannen

```csharp
// Alt (TableWindow.razor, AnwesenheitWindow.razor):
var hours = (zeitBis - zeitVon).TotalHours;
Buchung.Zeit = Convert.ToDecimal(hours.ToString("N2"));

// Neu:
var minutes = (int)(zeitBis - zeitVon).TotalMinutes;
Buchung.Zeit = minutes;
```

#### Pause-Berechnung (AnwesenheitWindow.razor)

```csharp
// Alt:
_pause = Convert.ToDecimal((grossHours - (double)_model.Zeit).ToString("N2"));

// Neu: Pause ebenfalls in Minuten
_pauseMinuten = (int)(zeitBis - zeitVon).TotalMinutes - (_model.Zeit ?? 0);
```

#### UI-Anzeige (ueberall)

```csharp
// Alt:
@buchung.Zeit?.ToString("N2") h

// Neu:
@buchung.Stunden?.ToString("N2") h
```

#### Eingabefelder

```csharp
// Alt:
<MudNumericField @bind-Value="@Buchung!.Zeit" Format="N2" />

// Neu (haengt vom Abrechnungsintervall ab):
<MudNumericField @bind-Value="@Buchung!.Zeit" Step="@_intervall" />
// Anzeige evtl. als "90 min" oder "1.50 h" je nach Praeferenz
```

#### Summen in Services

```csharp
// Alt:
buchungen.Sum(b => b.Zeit)  // Ergebnis in Stunden

// Neu — je nach Kontext:
buchungen.Sum(b => b.Zeit)       // Ergebnis in Minuten
buchungen.Sum(b => b.Stunden)    // Ergebnis in Stunden (fuer Anzeige/Saldo)
```

#### Saldo-Berechnungen (Stunden zu Tage)

```csharp
// Alt (DashboardService.cs):
buchungenSum /= arbeitszeit;  // Stunden / Tagesstunden = Tage

// Neu:
buchungenSumStunden = buchungen.Sum(b => b.Stunden);
buchungenSumStunden /= arbeitszeit;
```

#### Validatoren

```csharp
// Alt (BuchungValidator.cs):
if ((zeit.Value * 100) % 25 != 0)
    context.AddFailure("Bitte nur viertelstündig raportieren.");

// Neu (dynamisch pro Kunde):
// intervall = 15 (Viertelstunde) oder 1 (minutengenau) vom Kunden
if (zeit.Value % intervall != 0)
    context.AddFailure($"Bitte nur in {intervall}-Minuten-Schritten raportieren.");
```

#### Ferien-Sonderregel

```csharp
// Alt:
.GreaterThanOrEqualTo(4)  // 4 Stunden

// Neu:
.GreaterThanOrEqualTo(240)  // 240 Minuten = 4 Stunden
```

#### Report-Formatierung (DokumentService.cs)

```csharp
// Alt:
$"{v.Summe:N2}"

// Neu:
$"{v.Summe / 60m:N2}"
// oder die .BuchungStunden-Property verwenden
```

### Phase 4: Kundenspezifisches Abrechnungsintervall

**TKunde.cs:**
```csharp
public int AbrechnungsIntervallMinuten { get; set; } = 15;  // Default: Viertelstunde
```

**SQL-Migration:**
```sql
ALTER TABLE dbo.t_kunde ADD COLUMN abrechnungsintervallminuten INT NOT NULL DEFAULT 15;
```

**BuchungValidator anpassen:**
Der Validator muss das Intervall des zugehoerigen Kunden kennen. Optionen:
- Intervall als Property auf dem Buchung-DTO mitfuehren
- Oder Validator erhaelt den Wert als Konstruktor-Parameter

**UI-Eingabefeld dynamisch:**
- Step-Groesse des NumericFields abhaengig vom Kundenintervall
- Label anpassen: "Zeit (Viertelstunden)" vs. "Zeit (Minuten)"

---

## Checkliste fuer die Umsetzung

- [ ] SQL-Migration schreiben (naechste Nummer nach letztem Script in `Data/Database/`)
- [ ] Vorab-Check: gibt es nicht-ganzzahlige Minutenwerte in der DB?
- [ ] `TBuchung.cs` — `Zeit` und `OriginalZeit` auf `int?` aendern
- [ ] `TAnwesenheit.cs` — `Zeit` auf `int?` aendern
- [ ] `Buchung.cs` — `Zeit` auf `int?`, berechnete Property `Stunden` hinzufuegen
- [ ] `Anwesenheit.cs` — gleiche Aenderung
- [ ] `ReportBuchung.cs` — `BuchungZeit` auf `int?`, berechnete Property
- [ ] `Buchungstag.cs` — `TotalHours` Berechnung anpassen
- [ ] `Mappings.cs` — pruefen ob AutoMapper int<->int korrekt mappt
- [ ] `BuchungsService.cs` — Saldo-Berechnungen anpassen
- [ ] `DashboardService.cs` — alle Sum/Division-Stellen anpassen
- [ ] `VorgangService.cs` — Summen anpassen
- [ ] `DokumentService.cs` — Report-Berechnungen und Formatierung anpassen
- [ ] `TableWindow.razor` — TotalHours -> TotalMinutes, Eingabefeld
- [ ] `AnwesenheitWindow.razor` — Berechnung und Eingabefeld
- [ ] `ZeiterfassungTable.razor` — alle Anzeigen auf `.Stunden` umstellen
- [ ] `Reports.razor` — Summenanzeige anpassen
- [ ] `Dashboard.razor` — Fortschrittsbalken und Anzeige anpassen
- [ ] `BuchungValidator.cs` — Intervallpruefung dynamisch machen, Ferien-Regel (240 min)
- [ ] `AnwesenheitValidator.cs` — gleiche Aenderung
- [ ] `TKunde.cs` / `Kunde.cs` — `AbrechnungsIntervallMinuten` hinzufuegen
- [ ] SQL-Migration fuer TKunde
- [ ] Manuell testen: Buchung erfassen, Anwesenheit, Reports, Dashboard, Saldo
