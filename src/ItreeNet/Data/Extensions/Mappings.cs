using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;

namespace ItreeNet.Data.Extensions
{
    /// <summary>
    /// Handgeschriebene Mappings zwischen EF-Entities (T-Präfix) und Domain-Models.
    /// ToModel: Entity → Model (geladene Navigationen werden mitgemappt).
    /// ToEntity: Model → Entity (nur Skalare und Fremdschlüssel, keine Navigationen).
    /// </summary>
    public static class Mappings
    {
        // ---------- Projektionen für EF-Abfragen ----------

        public static readonly Expression<Func<TApplikation, Applikation>> ApplikationProjection = a => new Applikation
        {
            Id = a.Id,
            Bezeichnung = a.Bezeichnung
        };

        public static readonly Expression<Func<TRelease, Release>> ReleaseProjection = r => new Release
        {
            Id = r.Id,
            ApplikationId = r.ApplikationId,
            Datum = r.Datum,
            Bezeichnung = r.Bezeichnung,
            ApplikationName = r.Applikation.Bezeichnung
        };

        // ---------- Anwesenheit ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Anwesenheit? ToModel(this TAnwesenheit? t) => t == null ? null : new Anwesenheit
        {
            Id = t.Id,
            MitarbeiterId = t.MitarbeiterId,
            Datum = t.Datum,
            Typ = t.Typ,
            Zeit = t.Zeit,
            ZeitVon = t.ZeitVon,
            ZeitBis = t.ZeitBis,
            Notiz = t.Notiz,
            Mitarbeiter = t.Mitarbeiter.ToModel()
        };

        public static TAnwesenheit ToEntity(this Anwesenheit m) => new()
        {
            Id = m.Id,
            MitarbeiterId = m.MitarbeiterId,
            Datum = m.Datum,
            Typ = m.Typ,
            Zeit = m.Zeit,
            ZeitVon = m.ZeitVon,
            ZeitBis = m.ZeitBis,
            Notiz = m.Notiz
        };

        // ---------- Arbeitszeit ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Arbeitszeit? ToModel(this TArbeitszeit? t) => t == null ? null : new Arbeitszeit
        {
            Id = t.Id,
            Jahr = t.Jahr,
            Monat = t.Monat,
            Tagesarbeitszeit = t.Tagesarbeitszeit,
            Zeit = t.Arbeitszeit
        };

        public static TArbeitszeit ToEntity(this Arbeitszeit m) => new()
        {
            Id = m.Id,
            Jahr = m.Jahr,
            Monat = m.Monat,
            Tagesarbeitszeit = m.Tagesarbeitszeit,
            Arbeitszeit = m.Zeit
        };

        // ---------- ArbeitszeitReduktion ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static ArbeitszeitReduktion? ToModel(this TArbeitszeitReduktion? t) => t == null ? null : new ArbeitszeitReduktion
        {
            Id = t.Id,
            Datum = t.Datum,
            Reduktion = t.Reduktion
        };

        public static TArbeitszeitReduktion ToEntity(this ArbeitszeitReduktion m) => new()
        {
            Id = m.Id,
            Datum = m.Datum,
            Reduktion = m.Reduktion
        };

        // ---------- Buchung ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Buchung? ToModel(this TBuchung? t) => t == null ? null : new Buchung
        {
            Id = t.Id,
            MitarbeiterId = t.MitarbeiterId,
            VorgangId = t.VorgangId,
            Datum = t.Datum,
            Zeit = t.Zeit,
            ZeitVon = t.ZeitVon,
            ZeitBis = t.ZeitBis,
            Stunden = t.Stunden,
            Buchungstext = t.Buchungstext,
            Abgerechnet = t.Abgerechnet,
            Provisorisch = t.Provisorisch,
            ChangedBy = t.ChangedBy,
            ChangedOn = t.ChangedOn,
            OriginalDatum = t.OriginalDatum,
            OriginalText = t.OriginalText,
            OriginalVorgangId = t.OriginalVorgangId,
            OriginalZeit = t.OriginalZeit,
            Mitarbeiter = t.Mitarbeiter.ToModel(),
            Vorgang = t.Vorgang.ToModel(),
            OriginalVorgang = t.OriginalVorgang.ToModel()
        };

        public static TBuchung ToEntity(this Buchung m) => new()
        {
            Id = m.Id,
            MitarbeiterId = m.MitarbeiterId,
            VorgangId = m.VorgangId,
            Datum = m.Datum,
            Zeit = m.Zeit,
            ZeitVon = m.ZeitVon,
            ZeitBis = m.ZeitBis,
            Stunden = m.Stunden,
            Buchungstext = m.Buchungstext,
            Abgerechnet = m.Abgerechnet,
            Provisorisch = m.Provisorisch,
            ChangedBy = m.ChangedBy,
            ChangedOn = m.ChangedOn,
            OriginalDatum = m.OriginalDatum,
            OriginalText = m.OriginalText,
            OriginalVorgangId = m.OriginalVorgangId,
            OriginalZeit = m.OriginalZeit
        };

        // ---------- FerienArbeitspensum ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static FerienArbeitspensum? ToModel(this TFerienArbeitspensum? t) => t == null ? null : new FerienArbeitspensum
        {
            Id = t.Id,
            MitarbeiterId = t.MitarbeiterId,
            FerienProJahr = t.FerienProJahr,
            Arbeitspensum = t.Arbeitspensum,
            GueltigAb = t.GueltigAb,
            Montag = t.Montag,
            Dienstag = t.Dienstag,
            Mittwoch = t.Mittwoch,
            Donnerstag = t.Donnerstag,
            Freitag = t.Freitag,
            Mitarbeiter = t.Mitarbeiter.ToModel()
        };

        public static TFerienArbeitspensum ToEntity(this FerienArbeitspensum m) => new()
        {
            Id = m.Id,
            MitarbeiterId = m.MitarbeiterId,
            FerienProJahr = m.FerienProJahr,
            Arbeitspensum = m.Arbeitspensum,
            GueltigAb = m.GueltigAb,
            Montag = m.Montag,
            Dienstag = m.Dienstag,
            Mittwoch = m.Mittwoch,
            Donnerstag = m.Donnerstag,
            Freitag = m.Freitag
        };

        // ---------- Frontendtest ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Frontendtest? ToModel(this TFrontendtest? t) => t == null ? null : new Frontendtest
        {
            Id = t.Id,
            StartDatum = t.StartDatum,
            EndDatum = t.EndDatum,
            Ergebnis = t.Ergebnis,
            Total = t.Total,
            Korrekt = t.Korrekt,
            Fehlerhaft = t.Fehlerhaft,
            LastCommit = t.LastCommit!,
            FrontendtestDetailListe = t.TFrontendtestDetail.Select(d => d.ToModel()).ToList()
        };

        public static FrontendtestOverview ToOverview(this TFrontendtest t) => new()
        {
            Id = t.Id,
            StartDatum = t.StartDatum,
            EndDatum = t.EndDatum,
            Ergebnis = t.Ergebnis,
            Total = t.Total,
            Korrekt = t.Korrekt,
            Fehlerhaft = t.Fehlerhaft,
            LastCommit = t.LastCommit,
            Meldung = t.Meldung
        };

        [return: NotNullIfNotNull(nameof(t))]
        public static FrontendtestDetail? ToModel(this TFrontendtestDetail? t) => t == null ? null : new FrontendtestDetail
        {
            Id = t.Id,
            FrontendtestId = t.FrontendtestId,
            TestName = t.TestName,
            Dauer = t.Dauer,
            Ergebnis = t.Ergebnis,
            StdOut = t.StdOut,
            ErrMessage = t.ErrMessage,
            StackTrace = t.StackTrace,
            FrontendtestBildListe = t.TFrontendtestBild.Select(b => b.ToModel()).ToList()
        };

        [return: NotNullIfNotNull(nameof(t))]
        public static FrontendtestBild? ToModel(this TFrontendtestBild? t) => t == null ? null : new FrontendtestBild
        {
            Id = t.Id,
            FrontendtestDetailId = t.FrontendtestDetailId,
            Verzeichnis = t.Verzeichnis,
            Bild = t.Bild
        };

        // ---------- Kunde ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Kunde? ToModel(this TKunde? t) => t == null ? null : new Kunde
        {
            Id = t.Id,
            TeamId = t.TeamId,
            Kundenname = t.Kundenname,
            Intern = t.Intern,
            Aktiv = t.Aktiv,
            Adresse = t.Adresse,
            Team = t.Team.ToModel()
        };

        public static TKunde ToEntity(this Kunde m) => new()
        {
            Id = m.Id,
            TeamId = m.TeamId,
            Kundenname = m.Kundenname!,
            Intern = m.Intern,
            Aktiv = m.Aktiv,
            Adresse = m.Adresse
        };

        // ---------- Mitarbeiter ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Mitarbeiter? ToModel(this TMitarbeiter? t) => t == null ? null : new Mitarbeiter
        {
            Id = t.Id,
            AzureId = t.AzureId?.ToString(),
            Nachname = t.Nachname,
            Vorname = t.Vorname,
            Email = t.Email,
            Eintritt = t.Eintritt,
            Austritt = t.Austritt,
            Intern = t.Intern,
            Aktiv = t.Aktiv
        };

        public static TMitarbeiter ToEntity(this Mitarbeiter m) => new()
        {
            Id = m.Id,
            AzureId = string.IsNullOrEmpty(m.AzureId) ? null : Guid.Parse(m.AzureId),
            Nachname = m.Nachname!,
            Vorname = m.Vorname!,
            Email = m.Email,
            Eintritt = m.Eintritt,
            Austritt = m.Austritt,
            Intern = m.Intern,
            Aktiv = m.Aktiv
        };

        // ---------- MitarbeiterSaldo ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static MitarbeiterSaldo? ToModel(this TMitarbeiterSaldo? t) => t == null ? null : new MitarbeiterSaldo
        {
            Id = t.Id,
            MitarbeiterId = t.MitarbeiterId,
            Jahr = t.Jahr,
            Monat = t.Monat,
            Soll = t.Soll,
            Ist = t.Ist,
            StundenSaldo = t.StundenSaldo,
            FerienSaldo = t.FerienSaldo
        };

        public static TMitarbeiterSaldo ToEntity(this MitarbeiterSaldo m) => new()
        {
            Id = m.Id,
            MitarbeiterId = m.MitarbeiterId,
            Jahr = m.Jahr,
            Monat = m.Monat,
            Soll = m.Soll,
            Ist = m.Ist,
            StundenSaldo = m.StundenSaldo,
            FerienSaldo = m.FerienSaldo
        };

        // ---------- MitarbeiterSaldoKorrektur ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static MitarbeiterSaldoKorrektur? ToModel(this TMitarbeiterSaldoKorrektur? t) => t == null ? null : new MitarbeiterSaldoKorrektur
        {
            Id = t.Id,
            MitarbeiterId = t.MitarbeiterId,
            Jahr = t.Jahr,
            Monat = t.Monat,
            Stunden = t.Stunden,
            Ferien = t.Ferien,
            Grund = t.Grund,
            CreatedBy = t.CreatedBy,
            CreatedOn = t.CreatedOn,
            Mitarbeiter = t.Mitarbeiter.ToModel(),
            CreatedByNavigation = t.CreatedByNavigation.ToModel()
        };

        public static TMitarbeiterSaldoKorrektur ToEntity(this MitarbeiterSaldoKorrektur m) => new()
        {
            Id = m.Id,
            MitarbeiterId = m.MitarbeiterId,
            Jahr = m.Jahr,
            Monat = m.Monat,
            Stunden = m.Stunden,
            Ferien = m.Ferien,
            Grund = m.Grund!,
            CreatedBy = m.CreatedBy,
            CreatedOn = m.CreatedOn
        };

        // ---------- PipelineRuns ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static PipelineRuns? ToModel(this TPipelineRuns? t) => t == null ? null : new PipelineRuns
        {
            Id = t.Id,
            ProjectName = t.ProjectName,
            PipelineName = t.PipelineName,
            Status = t.Status,
            Result = t.Result,
            Link = t.Link,
            LastExecution = t.LastExecution
        };

        public static TPipelineRuns ToEntity(this PipelineRuns m) => new()
        {
            Id = m.Id,
            ProjectName = m.ProjectName,
            PipelineName = m.PipelineName,
            Status = m.Status,
            Result = m.Result,
            Link = m.Link,
            LastExecution = m.LastExecution
        };

        // ---------- Profil ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Profil? ToModel(this TProfil? t) => t == null ? null : new Profil
        {
            Id = t.Id,
            Mitarbeiterid = t.MitarbeiterId,
            Wert = t.Wert
        };

        public static TProfil ToEntity(this Profil m) => new()
        {
            Id = m.Id,
            MitarbeiterId = m.Mitarbeiterid,
            Wert = m.Wert!
        };

        // ---------- Projekt ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Projekt? ToModel(this TProjekt? t) => t == null ? null : new Projekt
        {
            Id = t.Id,
            KundeId = t.KundeId,
            Nummer = t.Nummer,
            Bezeichnung = t.Bezeichnung,
            Aktiv = t.Aktiv,
            Gesamtkosten = t.Gesamtkosten,
            Pauschal = t.Pauschal,
            Vertragsdauer = t.Vertragsdauer,
            Mehrwertsteuer = t.Mehrwertsteuer,
            BuchungsintervallMinuten = t.BuchungsintervallMinuten,
            Gesamtzeit = t.Gesamtzeit,
            EmailGesendet80 = t.EmailGesendet80,
            EmailGesendet90 = t.EmailGesendet90,
            EmailGesendet100 = t.EmailGesendet100,
            Kunde = t.Kunde.ToModel()
        };

        public static TProjekt ToEntity(this Projekt m) => new()
        {
            Id = m.Id,
            KundeId = m.KundeId,
            Nummer = m.Nummer!,
            Bezeichnung = m.Bezeichnung!,
            Aktiv = m.Aktiv,
            Gesamtkosten = m.Gesamtkosten,
            Pauschal = m.Pauschal,
            Vertragsdauer = m.Vertragsdauer,
            Mehrwertsteuer = m.Mehrwertsteuer,
            BuchungsintervallMinuten = m.BuchungsintervallMinuten,
            Gesamtzeit = m.Gesamtzeit,
            EmailGesendet80 = m.EmailGesendet80,
            EmailGesendet90 = m.EmailGesendet90,
            EmailGesendet100 = m.EmailGesendet100
        };

        // ---------- Spesen ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Spesen? ToModel(this TSpesen? t) => t == null ? null : new Spesen
        {
            Id = t.Id,
            MitarbeiterId = t.MitarbeiterId,
            Datum = t.Datum,
            Spesenart = t.Spesenart,
            AnlassOrt = t.AnlassOrt,
            Betrag = t.Betrag,
            Kreditkarte = t.Kreditkarte,
            EingereichtAm = t.EingereichtAm,
            Mitarbeiter = t.Mitarbeiter.ToModel()
        };

        public static TSpesen ToEntity(this Spesen m) => new()
        {
            Id = m.Id,
            MitarbeiterId = m.MitarbeiterId,
            Datum = m.Datum,
            Spesenart = m.Spesenart,
            AnlassOrt = m.AnlassOrt,
            Betrag = m.Betrag,
            Kreditkarte = m.Kreditkarte,
            EingereichtAm = m.EingereichtAm
        };

        // ---------- Team ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Team? ToModel(this TTeam? t) => t == null ? null : new Team
        {
            Id = t.Id,
            Bezeichnung = t.Bezeichnung,
            NaturalId = t.NaturalId,
            Sort = t.Sort,
            Aktiv = t.Aktiv
        };

        public static TTeam ToEntity(this Team m) => new()
        {
            Id = m.Id,
            Bezeichnung = m.Bezeichnung!,
            NaturalId = m.NaturalId!,
            Sort = checked((byte)(m.Sort ?? 0)),
            Aktiv = m.Aktiv
        };

        // ---------- Vorgang ----------

        [return: NotNullIfNotNull(nameof(t))]
        public static Vorgang? ToModel(this TVorgang? t) => t == null ? null : new Vorgang
        {
            Id = t.Id,
            ProjektId = t.ProjektId,
            Bezeichnung = t.Bezeichnung,
            Aktiv = t.Aktiv,
            Ferien = t.Ferien,
            Gleitzeit = t.Gleitzeit,
            Stundenansatz = t.Stundenansatz,
            AnzahlStunden = t.AnzahlStunden,
            Projekt = t.Projekt.ToModel()
        };

        public static TVorgang ToEntity(this Vorgang m) => new()
        {
            Id = m.Id,
            ProjektId = m.ProjektId,
            Bezeichnung = m.Bezeichnung,
            Aktiv = m.Aktiv,
            Ferien = m.Ferien,
            Gleitzeit = m.Gleitzeit,
            Stundenansatz = m.Stundenansatz,
            AnzahlStunden = m.AnzahlStunden
        };
    }
}
