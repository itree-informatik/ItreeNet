# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Interne Webanwendung der itree GmbH für Zeiterfassung, Kunden- und Mitarbeiterverwaltung. **Blazor Server** (.NET 10) mit MudBlazor/ItreeMud, PostgreSQL und Azure-AD-Login.

**Achtung — weicht vom Muster der Schwesterprojekte ab** (siehe `C:\repos\CLAUDE.md`): kein ItreeBlazor-Framework, kein Telerik, kein WASM, keine 5-Projekt-Struktur. Ein einziges Projekt (`src/ItreeNet/ItreeNet.csproj`), Solution im `.slnx`-Format (XML — keine Text-Manipulation, `dotnet sln`/`XDocument` verwenden), alles läuft serverseitig über SignalR — keine separate Web-API (einziger Controller: `LoginController`).

## Befehle

```bash
dotnet build src/ItreeNet.slnx
dotnet run --project src/ItreeNet/ItreeNet.csproj

# Docker (build-args für NuGet-Token + Lizenzkeys nötig, siehe .github/workflows/build.yml)
docker build -f src/Dockerfile -t itreenet src/
```

**Es gibt kein Testprojekt** — keine Tests in diesem Repo.

**NuGet:** `src/nuget.config` enthält nur nuget.org. Das Paket `ItreeMud` kommt vom GitHub-Packages-Feed `itree-informatik` — die CI fügt ihn per `dotnet nuget add source` hinzu; lokal muss er in der globalen NuGet-Konfiguration vorhanden sein.

**Start-Voraussetzungen:** User Secrets mit `ConnectionStrings:APP` (PostgreSQL), `AzureAd`, `Bosses`, `File:Store` und `LicenseKeys` (AutoMapper **und** GemBox sind Pflicht — `Program.cs` wirft sonst beim Start). Beispielstruktur der `secrets.json` steht im README.

## Architektur

### Vertical Slice pro Feature

Alles liegt in einem Projekt; pro Feature existiert diese Kette:

```
Data/Models/DB/T{Name}.cs        EF-Entity (T-Präfix, DB-first, kein EF-Migrations)
Data/Models/{Name}.cs            Domain-Model (ohne Präfix)
Data/Extensions/Mappings.cs      EIN zentrales AutoMapper-Profil für alle Mappings
Data/Validators/{Name}Validator.cs   FluentValidation
Interfaces/I{Name}Service.cs  →  Services/{Name}Service.cs
Program.cs                       manuelle Scoped-Registrierung jedes Services
```

Es gibt **kein** generisches `BaseActionService`/`BaseActionController`-Muster wie in den Schwesterprojekten — Services sind handgeschrieben.

### Datenbank

- `ZeiterfassungContext` (`Data/Models/DB/zeiterfassungContext.cs`) ist DB-first gescaffoldet; Schemaänderungen erfolgen direkt in der Datenbank, es gibt keine Migrationsskripte im Repo.
- Zugriff **immer** über `IDbContextFactory<ZeiterfassungContext>`: `await using var context = await _dbFactory.CreateDbContextAsync();` — kurzlebige Kontexte pro Operation (Blazor-Server-Muster).
- Serilog loggt in die Tabelle `TLog` derselben PostgreSQL-Datenbank (Schema `dbo`).

### Auth & Berechtigungen

- Azure AD via Microsoft.Identity.Web (OIDC) + Microsoft Graph.
- `Middleware/UserInfoClaims.cs` (`IClaimsTransformation`) mappt die Azure-`uid` auf `TMitarbeiter.AzureId` und setzt den `IsIntern`-Claim; darauf basiert die Policy `internPolicy`.
- Alle internen Seiten liegen unter der Route `/intern/...` mit `@attribute [Authorize]`.
- Feingranulare Checks passieren **in den Services** über `UserService.CurrentUser` (`Benutzer` mit `IsAdmin`, `IsIntern`, `MitarbeiterId`). Admin = Azure-ID in `Globals.BossList` (aus Config-Sektion `Bosses`).
- Statischer globaler Zustand in `Data/Extensions/Globals.cs` (`BossList`, `FileStorePath`).

### UI & Übersetzungen

- MudBlazor plus hauseigenes NuGet-Paket **ItreeMud** (`ItreeFormWindow`, `ItreeButtonGroup`, `ItreeFormDropdown`, `ITranslationProvider`, `LanguageService`, …). Formulare/Dialoge werden bevorzugt mit diesen Itree*-Komponenten gebaut.
- Übersetzungen sind **hartkodiert** im Dictionary in `Data/Extensions/ItreeNetTranslationProvider.cs` (SCREAMING_SNAKE_CASE-Keys, nur Deutsch) — nicht datenbankgetrieben wie in den Schwesterprojekten. Neue UI-Texte dort ergänzen.

### Hintergrundarbeit

`IBackgroundTaskQueue` + `BackgroundQueueHostedService` für asynchrone Jobs (z. B. Azure-DevOps-Pipeline-Statusabfragen über die `Pipelines`-Konfiguration mit PAT).

## CI/CD

`.github/workflows/build.yml`, vier Stufen: Versioning (Patch aus `release-1.0.*`-Git-Tags) → Build → Publish (nur main: Docker → Azure Container Registry, SBOM via CycloneDX/syft → Dependency-Track, Release-Tag) → Deploy (Neustart der Azure Web App `itree-website`).
