# ItreeNet

Interne Webanwendung zur **Zeiterfassung, Kunden- und Mitarbeiterverwaltung** der itree GmbH.

## Tech-Stack

| Komponente | Technologie |
|---|---|
| Frontend & Backend | **Blazor Server** (.NET 10) |
| UI-Framework | MudBlazor 9 |
| Datenbank | SQL Server (EF Core 10) |
| Authentifizierung | Azure AD (Microsoft Identity Web / OIDC) |
| Logging | Serilog (Console + SQL Server) |
| Validierung | FluentValidation |
| Mapping | AutoMapper |
| Dokumentenerzeugung | GemBox.Document, ClosedXML |
| E-Mail | Azure Communication Services |
| Deployment | Docker Container → Azure Web App |
| CI/CD | Azure DevOps (Multi-Stage Pipeline) |

## Voraussetzungen

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server Instanz

## Schnellstart

```bash
# Repository klonen
git clone https://github.com/itree-informatik/ItreeNet.git
cd ItreeNet

# NuGet-Pakete wiederherstellen & bauen
dotnet build src/ItreeNet.sln

# Anwendung starten
dotnet run --project src/ItreeNet/ItreeNet.csproj
```

### Docker

```bash
docker build -f src/Dockerfile -t itreenet src/

docker run -p 8080:8080 itreenet
```

## Projektstruktur

```
src/ItreeNet/
├── Pages/                  # Razor Pages (Kunden/, Mitarbeiter/, Zeiterfassung/)
├── Shared/                 # Wiederverwendbare Blazor-Komponenten & Form Builder
├── Services/               # Business-Logik
├── Interfaces/             # Service-Verträge
├── Data/
│   ├── Models/
│   │   └── DB/             # EF Core Entities (T-Präfix, z. B. TMitarbeiter)
│   ├── Validators/         # FluentValidation-Regeln
│   ├── Extensions/         # AutoMapper-Profile, Hilfsmethoden
│   └── Database/           # SQL-Migrationsskripte (01–12)
├── Middleware/             # UserInfoClaims (Azure AD → IsIntern)
└── wwwroot/                # Statische Dateien
```

## Architektur

Die Anwendung läuft vollständig serverseitig via **SignalR** — kein WebAssembly, keine separate API.

- **Authentifizierung:** Azure AD (OIDC). Ein Custom-Middleware ordnet den angemeldeten Benutzer einem `TMitarbeiter`-Datensatz zu und setzt den `IsIntern`-Claim.
- **Autorisierung:** Interne Seiten (`Pages/Intern/`) erfordern `IsIntern=true`.
- **Datenbank:** EF Core mit `DbContextFactory` — Services erzeugen kurzlebige Kontexte via `CreateDbContext()`.
- **Migrationen:** SQL-Skripte in `Data/Database/`, keine EF-Migrationen.

## CI/CD

GitHub Actions Pipeline (`.github/workflows/build-itree.yml`) mit vier Jobs:

1. **Versioning** — Semantische Versionierung aus Branch/Tag
2. **Build** — `dotnet restore` + `dotnet build`
3. **Publish** *(nur main)* — Docker Build → Azure Container Registry, SBOM-Generierung
4. **Deploy** *(nur main)* — Neustart der Azure Web App `itree-website`

## Lizenz

Proprietär — nur zur internen Nutzung.
