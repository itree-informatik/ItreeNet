using AutoMapper;
using ItreeNet.Data.Enums;
using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ItreeNet.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;
        private readonly IMitarbeiterService _mitarbeiterService;
        private readonly IVorgangService _vorgangService;
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        private const string Organization = "itreeCH";
        private readonly List<string>? _projects;
        private bool _pipelinesEnabled;
        private int _updateInHours = 4;

        public DashboardService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper, IMitarbeiterService mitarbeiterService, IVorgangService vorgangService, IConfiguration config)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
            _mitarbeiterService = mitarbeiterService;
            _vorgangService = vorgangService;

            _projects = config["Pipelines:Projects"]?.Split(",").ToList();
            var pat = config["Pipelines:PAT"];
            var updateInHours = config["Pipelines:UpdateInHours"];

            if (!string.IsNullOrEmpty(pat) && _projects != null && _projects.Any())
            {
                _pipelinesEnabled = true;
                var base64Token = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{pat}"));
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Token);

                if (!string.IsNullOrEmpty(updateInHours))
                {
                    var parsed = Int32.TryParse(updateInHours, out int result);
                    if (parsed)
                    {
                        _updateInHours = result;
                    }
                }
            }
        }

        public async Task<List<DashboardMitarbeiter>> GetDashboardMitarbeiterAsync()
        {
            var list = await _mitarbeiterService.GetAllActiveAsync();
            list = list.Where(m => !Globals.BossList.Contains(m.AzureId!)).OrderByDescending(m => m.Intern).ThenBy(m => m.Fullname).ToList();

            var dashboardList = new List<DashboardMitarbeiter>();

            foreach (var model in list)
            {
                var dashboardModel = new DashboardMitarbeiter()
                {
                    Id = model.Id,
                    Intern = model.Intern,
                    Fullname = model.Fullname,
                    FerienSaldo = await GetFerienSaldoByMitarbeiterAsnyc(model.Id),
                    StundenSaldo = await GetStundenSaldoByMitarbeiterAsync(model.Id),
                    BuchungAbgeschlossen = await IsAbschlussDoneAsync(model.Id)
                };

                dashboardList.Add(dashboardModel);
            }

            return dashboardList;
        }

        public async Task<decimal> GetFerienSaldoByMitarbeiterAsnyc(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var saldo = decimal.Zero;
            var neuesJahr = false;

            var ferienSaldoModel = await context.TMitarbeiterSaldo
                .AsNoTracking()
                .Where(m => m.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(m => m.Jahr)
                .ThenByDescending(m => m.Monat)
                .FirstOrDefaultAsync();
            if (ferienSaldoModel != null)
            {
                saldo = ferienSaldoModel.FerienSaldo;
                neuesJahr = ferienSaldoModel.Jahr < DateTime.Now.Year;
            }

            // Im Januar muss Saldo um Ferien pro Jahr erhöht werden
            if (neuesJahr)
            {
                var fap = await context.TFerienArbeitspensum
                    .AsNoTracking()
                    .Where(f => f.MitarbeiterId == mitarbeiterId)
                    .OrderByDescending(f => f.GueltigAb)
                    .FirstOrDefaultAsync();
                if (fap != null)
                {
                    saldo += fap.FerienProJahr;
                }
            }

            return saldo;
        }

        public async Task<decimal> GetFerienAktuellesSaldoByMitarbeiterAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var saldo = decimal.Zero;
            var neuesJahr = false;

            var ferienSaldoModel = await context.TMitarbeiterSaldo
                .AsNoTracking()
                .Where(m => m.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(m => m.Jahr)
                .ThenByDescending(m => m.Monat)
                .FirstOrDefaultAsync();
            if (ferienSaldoModel != null)
            {
                neuesJahr = ferienSaldoModel.Jahr < DateTime.Now.Year;
                saldo = ferienSaldoModel.FerienSaldo;

                var date1 = new DateOnly(ferienSaldoModel.Jahr, ferienSaldoModel.Monat,
                    DateTime.DaysInMonth(ferienSaldoModel.Jahr, ferienSaldoModel.Monat));

                // Arbeitszeit und Arbeitspensum laden (für Stunden → Tage Umrechnung)
                var arbeitszeitModel = await context.TArbeitszeit
                    .AsNoTracking()
                    .SingleOrDefaultAsync(a => a.Jahr == DateTime.Now.Year && a.Monat == DateTime.Now.Month)
                    ?? throw new InvalidDataException($"Keine Arbeitszeit für {DateTime.Now.Month}/{DateTime.Now.Year} konfiguriert");

                var arbeitszeit = arbeitszeitModel.Tagesarbeitszeit;

                var arbeitspensum = await context.TFerienArbeitspensum
                    .AsNoTracking()
                    .Where(a => a.MitarbeiterId == mitarbeiterId && a.GueltigAb <= DateOnly.FromDateTime(DateTime.Today))
                    .OrderByDescending(a => a.GueltigAb).FirstOrDefaultAsync()
                    ?? throw new InvalidDataException($"Kein Arbeitspensum für Mitarbeiter {mitarbeiterId} gefunden");

                if (arbeitspensum.Arbeitspensum != 100 && arbeitspensum.Montag && arbeitspensum.Dienstag && arbeitspensum.Mittwoch && arbeitspensum.Donnerstag && arbeitspensum.Freitag)
                {
                    arbeitszeit *= (arbeitspensum.Arbeitspensum / 100);
                }

                // 1) Ferien über Buchungen (Ferien-Vorgang)
                var ferienAktivitaet = await context.TVorgang.AsNoTracking().SingleOrDefaultAsync(v => v.Ferien)
                    ?? throw new InvalidDataException("Kein Ferien-Vorgang konfiguriert");

                var buchungen = await context.TBuchung.AsNoTracking()
                    .Where(b => b.Datum > date1
                                && b.VorgangId == ferienAktivitaet.Id
                                && b.MitarbeiterId == mitarbeiterId)
                    .ToListAsync();

                if (buchungen.Count > 0)
                {
                    var buchungenSum = buchungen.Sum(b => b.Zeit);
                    buchungenSum /= arbeitszeit;
                    saldo -= buchungenSum ?? decimal.Zero;
                }

                // 2) Ferien über Anwesenheit (Typ = Ferien)
                var ferienAnwesenheiten = await context.TAnwesenheit.AsNoTracking()
                    .Where(a => a.Datum > date1
                                && a.Typ == EnumAnwesenheitTyp.Ferien
                                && a.MitarbeiterId == mitarbeiterId)
                    .ToListAsync();

                if (ferienAnwesenheiten.Count > 0)
                {
                    var anwesenheitSum = ferienAnwesenheiten.Sum(a => a.Zeit) ?? decimal.Zero;
                    anwesenheitSum /= arbeitszeit;
                    saldo -= anwesenheitSum;
                }
            }

            // Im Januar muss Saldo um Ferien pro Jahr erhöht werden
            if (neuesJahr)
            {
                var fap = await context.TFerienArbeitspensum
                    .AsNoTracking()
                    .Where(f => f.MitarbeiterId == mitarbeiterId)
                    .OrderByDescending(f => f.GueltigAb)
                    .FirstOrDefaultAsync();
                if (fap != null)
                {
                    saldo += fap.FerienProJahr;
                }
            }

            return saldo;
        }

        public async Task<decimal> GetStundenSaldoByMitarbeiterAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var stundenSaldo = await context.TMitarbeiterSaldo
                .AsNoTracking()
                .Where(m => m.MitarbeiterId == mitarbeiterId)
                .OrderByDescending(m => m.Jahr)
                .ThenByDescending(m => m.Monat)
                .Select(m => (decimal?)m.StundenSaldo)
                .FirstOrDefaultAsync();

            return stundenSaldo ?? 0;
        }

        public async Task<DashboardAktuellesSaldo?> GetStundenAktuellesSaldoByMitarbeiterAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var stundenSaldoModel = await context.TMitarbeiterSaldo
                                    .AsNoTracking()
                                    .Where(m => m.MitarbeiterId == mitarbeiterId)
                                    .OrderByDescending(m => m.Jahr)
                                    .ThenByDescending(m => m.Monat)
                                    .FirstOrDefaultAsync();

            if (stundenSaldoModel != null)
            {
                var saldo = stundenSaldoModel.StundenSaldo;

                var anwesenheitStartDate = new DateOnly(stundenSaldoModel.Jahr, stundenSaldoModel.Monat,
                    DateTime.DaysInMonth(stundenSaldoModel.Jahr, stundenSaldoModel.Monat));

                var anwesenheiten = await context.TAnwesenheit
                                                .AsNoTracking()
                                                .Where(a => a.Datum > anwesenheitStartDate &&
                                                            a.MitarbeiterId == mitarbeiterId &&
                                                            a.Typ != EnumAnwesenheitTyp.Gleitzeit)
                                                .ToListAsync();

                if (anwesenheiten.Count > 0)
                {
                    var istStunden = anwesenheiten.Sum(a => a.Zeit) ?? Decimal.Zero;

                    decimal sollStunden;

                    var startDay = anwesenheitStartDate.AddDays(1);

                    var endDay = anwesenheiten.OrderByDescending(a => a.Datum).First().Datum;

                    decimal anzahlTage = 0;

                    var arbeitspensum = await context.TFerienArbeitspensum
                                        .AsNoTracking()
                                        .Where(a => a.MitarbeiterId == mitarbeiterId &&
                                                    a.GueltigAb <= DateOnly.FromDateTime(DateTime.Today))
                                        .OrderByDescending(a => a.GueltigAb)
                                        .FirstOrDefaultAsync()
                                        ?? throw new InvalidDataException($"Kein Arbeitspensum für Mitarbeiter {mitarbeiterId} gefunden");

                    var arbeitszeitModel = await context.TArbeitszeit
                                                .AsNoTracking()
                                                .SingleOrDefaultAsync(a => a.Jahr == DateTime.Today.Year &&
                                                    a.Monat == DateTime.Today.Month)
                                                ?? throw new InvalidDataException($"Keine Arbeitszeit für {DateTime.Today.Month}/{DateTime.Today.Year} konfiguriert");

                    var arbeitszeit = arbeitszeitModel.Tagesarbeitszeit;

                    var reduktionenDict = await context.TArbeitszeitReduktion
                        .AsNoTracking()
                        .Where(r => r.Datum >= startDay && r.Datum <= endDay)
                        .ToDictionaryAsync(r => r.Datum, r => r.Reduktion);

                    for (var day = startDay; day <= endDay; day = day.AddDays(1))
                    {
                        if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
                            continue;

                        if (!arbeitspensum.Montag && day.DayOfWeek == DayOfWeek.Monday)
                            continue;

                        if (!arbeitspensum.Dienstag && day.DayOfWeek == DayOfWeek.Tuesday)
                            continue;

                        if (!arbeitspensum.Mittwoch && day.DayOfWeek == DayOfWeek.Wednesday)
                            continue;

                        if (!arbeitspensum.Donnerstag && day.DayOfWeek == DayOfWeek.Thursday)
                            continue;

                        if (!arbeitspensum.Freitag && day.DayOfWeek == DayOfWeek.Friday)
                            continue;

                        if (!reduktionenDict.TryGetValue(day, out var reduktion))
                        {
                            anzahlTage++;
                        }
                        else if (reduktion < arbeitszeit)
                        {
                            anzahlTage += reduktion / arbeitszeit;
                        }
                    }

                    if (arbeitspensum.Arbeitspensum != 100 && arbeitspensum.Montag && arbeitspensum.Dienstag && arbeitspensum.Mittwoch && arbeitspensum.Donnerstag && arbeitspensum.Freitag)
                    {
                        arbeitszeit *= (arbeitspensum.Arbeitspensum / 100);
                    }

                    sollStunden = anzahlTage * arbeitszeit;

                    saldo += istStunden - sollStunden;

                    return new DashboardAktuellesSaldo() { Datum = endDay, Stunden = decimal.Round(saldo, 2) };
                }
            }

            return null;
        }

        public async Task<bool> IsAbschlussDoneAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var previousMonth = DateTime.Now.AddMonths(-1);

            return await context.TMitarbeiterSaldo
                .AnyAsync(m => m.MitarbeiterId == mitarbeiterId &&
                               m.Jahr == previousMonth.Year &&
                               m.Monat == previousMonth.Month);
        }

        public async Task<List<Buchung>> TopBuchungenAsync(Guid mitarbeiterId)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var currentYear = DateTime.Today.Year;
            var firstDay = new DateOnly(currentYear, 1, 1);
            var lastDay = new DateOnly(currentYear, 12, 31);

            var list = await context.TBuchung
                .AsNoTracking()
                .Include(x => x.Vorgang)
                .ThenInclude(x => x.Projekt)
                .ThenInclude(x => x.Kunde)
                .Where(b => b.MitarbeiterId == mitarbeiterId && firstDay <= b.Datum &&
                            b.Datum <= lastDay)
                .ToListAsync();

            var groups = list.GroupBy(b => b.VorgangId).ToList();

            List<Buchung> topBuchungen = new List<Buchung>();

            foreach (var group in groups)
            {
                var tBuchungen = list.Where(b => b.VorgangId == group.Key).ToList();
                var vorgang = _mapper.Map<Vorgang>(tBuchungen.First().Vorgang);

                Buchung neu = new()
                {
                    VorgangId = group.Key,
                    Vorgang = vorgang,
                    Zeit = tBuchungen.Sum(b => b.Zeit),
                    ProjektName = vorgang.Projekt?.Bezeichnung,
                    KundenName = vorgang.Projekt?.Kunde?.Kundenname
                };
                topBuchungen.Add(neu);
            }
            return topBuchungen.OrderByDescending(b => b.Zeit).Take(5).ToList();
        }

        public async Task<List<ProvisorischeBuchung>> GetProvisorischeBuchungenAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            return await context.TBuchung
                .AsNoTracking()
                .Where(b => b.Provisorisch)
                .OrderByDescending(b => b.Datum)
                .Select(b => new ProvisorischeBuchung
                {
                    Id = b.Id,
                    Datum = b.Datum,
                    Zeit = b.Zeit,
                    Buchungstext = b.Buchungstext,
                    ProjektName = b.Vorgang!.Projekt!.Bezeichnung,
                    VorgangName = b.Vorgang.Bezeichnung,
                    MitarbeiterName = b.Mitarbeiter!.Vorname + " " + b.Mitarbeiter.Nachname
                })
                .ToListAsync();
        }

        public async Task<List<DashboardProjekt>> GetProjectBookingsAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tProjekte = await context.TProjekt
                .AsNoTracking()
                .Include(p => p.Kunde)
                .ThenInclude(p => p.Team)
                .Include(p => p.TVorgang)
                .Where(x => x.Aktiv == true && !x.Kunde.Intern && x.TVorgang.Where(v => v.AnzahlStunden > 0 && v.Aktiv).Sum(v => v.AnzahlStunden) > 0)
                .ToListAsync();

            var allVorgangIds = tProjekte
                .SelectMany(p => p.TVorgang.Where(v => v.Aktiv && v.AnzahlStunden > decimal.Zero))
                .Select(v => v.Id)
                .ToList();

            var bookedHoursDict = await context.TBuchung
                .AsNoTracking()
                .Where(b => allVorgangIds.Contains(b.VorgangId))
                .GroupBy(b => b.VorgangId)
                .Select(g => new { VorgangId = g.Key, Stunden = g.Sum(b => b.Zeit) })
                .ToDictionaryAsync(x => x.VorgangId, x => x.Stunden ?? decimal.Zero);

            var dashboardProjects = new List<DashboardProjekt>();
            foreach (var projekt in tProjekte)
            {
                var relevantVorgaenge = projekt.TVorgang.Where(a => a.Aktiv && a.AnzahlStunden > decimal.Zero).ToList();

                var dashboardprojekt = new DashboardProjekt
                {
                    Id = projekt.Id,
                    Name = projekt.Bezeichnung,
                    KundenId = projekt.KundeId,
                    KundenName = projekt.Kunde.Kundenname,
                    Team = projekt.Kunde.Team?.Bezeichnung,
                    AnzahlStunden = relevantVorgaenge.Sum(v => v.AnzahlStunden),
                    GebuchteStunden = relevantVorgaenge.Sum(v => bookedHoursDict.TryGetValue(v.Id, out var h) ? h : decimal.Zero)
                };

                dashboardProjects.Add(dashboardprojekt);
            }

            return dashboardProjects.OrderByDescending(b => b.Prozent).ToList();
        }

        public async Task<List<PipelineRuns>> GetPipelineRunsAsync()
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var tPipelineRuns = await context.TPipelineRuns
                .AsNoTracking()
                .GroupBy(pr => new { pr.ProjectName, pr.PipelineName })
                .Select(g => g.OrderByDescending(pr => pr.LastExecution).FirstOrDefault())
                .ToListAsync();

            List<PipelineRuns> unsortedList;

            if (tPipelineRuns.Any(p => p != null && p.LastExecution < DateTime.UtcNow.AddHours(-_updateInHours)) || tPipelineRuns.Count == 0)
            {
                unsortedList = await RefreshPipelines(context);
            }
            else
            {
                unsortedList = _mapper.Map<List<PipelineRuns>>(tPipelineRuns);
            }

            var res = new List<PipelineRuns>();

            res.AddRange(unsortedList.Where(r => r.Result == "failed").OrderBy(r => r.ProjectName).ThenBy(r => r.ProjectName));
            res.AddRange(unsortedList.Where(r => r.Result == "canceled").OrderBy(r => r.ProjectName).ThenBy(r => r.ProjectName));
            res.AddRange(unsortedList.Where(r => r.Result == "unknown").OrderBy(r => r.ProjectName).ThenBy(r => r.ProjectName));
            res.AddRange(unsortedList.Where(r => r.Result == "succeeded").OrderBy(r => r.ProjectName).ThenBy(r => r.ProjectName));

            return res;
        }

        private async Task<List<PipelineRuns>> RefreshPipelines(ZeiterfassungContext context)
        {
            var list = new List<PipelineRuns>();

            if (_pipelinesEnabled)
            {
                var saveNewList = new List<TPipelineRuns>();

                foreach (var project in _projects!)
                {
                    var name = project.Split("$")[0];
                    var pipelineId = project.Split("$")[1];

                    var url = $"https://dev.azure.com/{Organization}/{name}/_apis/pipelines/{pipelineId}/runs?api-version=7.1-preview.1&$orderby=createdDate desc&$filter=state eq 'completed'";
                    var response = await Client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var pipelineRunResponse = JsonSerializer.Deserialize<PipelineRunResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        var latestRun = (pipelineRunResponse?.Value)?.Where(p => p.State == "completed").MaxBy(run => run.CreatedDate);

                        if (latestRun != null)
                        {
                            var tPipelineRunes = new TPipelineRuns();
                            tPipelineRunes.LastExecution = DateTime.UtcNow;
                            tPipelineRunes.Id = Guid.NewGuid();
                            tPipelineRunes.Link = latestRun._links!.Web!.Href!;
                            tPipelineRunes.PipelineName = latestRun.Pipeline!.Name!;
                            tPipelineRunes.ProjectName = name;
                            tPipelineRunes.Status = latestRun.State!;
                            tPipelineRunes.Result = latestRun.Result!;

                            saveNewList.Add(tPipelineRunes);
                        }
                    }
                }
                if (saveNewList.Any())
                {
                    await context.TPipelineRuns.AddRangeAsync(saveNewList);
                    await context.SaveChangesAsync();
                }

                list = _mapper.Map<List<PipelineRuns>>(saveNewList);
            }

            return list;
        }

        // First extern, Second intern
        public async Task<PerformanceList> GetProductivity(Guid mitarbeiterId, int jahr = 0)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            if (jahr == 0)
                jahr = DateTime.Now.Year;

            var date = new DateOnly(jahr, 1, 1);

            var myQuery = context.TBuchung
                .AsNoTracking()
                .Where(b => b.MitarbeiterId == mitarbeiterId && b.Datum >= date);

            // externe buchungen, also alle, die nicht im Kunden itree erstellt worden sind
            var extBuchungen = await myQuery
                .Where(b => !b.Vorgang.Projekt.Kunde.Intern)
                .Select(b => b.Zeit)
                .SumAsync() ?? decimal.Zero;

            // interne buchungen, ausser Ferien und Krankheit
            var intBuchungen = await myQuery
                .Where(b => b.Vorgang.Projekt.Kunde.Intern && b.Vorgang.Projekt.Bezeichnung != "Abwesenheiten")
                .Select(b => b.Zeit)
                .SumAsync() ?? decimal.Zero;

            var totalBuchungen = extBuchungen + intBuchungen;
            var extProzent = totalBuchungen > 0 ? (extBuchungen / totalBuchungen) * 100 : 0;
            var intProzent = totalBuchungen > 0 ? (intBuchungen / totalBuchungen) * 100 : 0;

            var extPerformance = new Performance
            {
                Name = "Extern",
                Wert = extBuchungen,
                Total = totalBuchungen,
                Prozent = extProzent,
                Class = "bg-primary"
            };

            var intPerformance = new Performance
            {
                Name = "Intern",
                Wert = intBuchungen,
                Total = totalBuchungen,
                Prozent = intProzent,
                Class = "bg-success"
            };

            var myList = new List<Performance> { extPerformance, intPerformance };

            var performance = new PerformanceList
            {
                Name = "Persönlich",
                Performances = myList
            };

            return performance;
        }

        public async Task<List<PerformanceList>> GetTeamProductivity(Guid mitarbeiterId, int jahr = 0)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            if (jahr == 0)
                jahr = DateTime.Now.Year;

            var date = new DateOnly(jahr, 1, 1);

            var myTeamIds = await context.TMitarbeiterTeam
                .Where(m => m.MitarbeiterId == mitarbeiterId)
                .Select(m => m.TeamId)
                .ToListAsync();

            var teams = await context.TTeam
                .AsNoTracking()
                .Where(t => myTeamIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

            var allTeamMembers = await context.TMitarbeiterTeam
                .Where(t => myTeamIds.Contains(t.TeamId))
                .Select(t => new { t.TeamId, t.MitarbeiterId })
                .ToListAsync();

            var performanceList = new List<PerformanceList>();

            foreach (var teamId in myTeamIds)
            {
                if (!teams.TryGetValue(teamId, out var team))
                    throw new InvalidDataException($"Team {teamId} nicht gefunden");

                var teamMitarbeiter = allTeamMembers
                    .Where(t => t.TeamId == teamId)
                    .Select(t => t.MitarbeiterId)
                    .ToList();

                var myQuery = context.TBuchung
                    .AsNoTracking()
                    .Where(b => b.Datum >= date && teamMitarbeiter.Contains(b.MitarbeiterId));

                // externe buchungen, also alle, die nicht im Kunden itree erstellt worden sind
                var extBuchungen = await myQuery
                    .Where(b => !b.Vorgang.Projekt.Kunde.Intern)
                    .Select(b => b.Zeit)
                    .SumAsync() ?? decimal.Zero;

                // interne buchungen, ausser Ferien und Krankheit
                var intBuchungen = await myQuery
                    .Where(b => b.Vorgang.Projekt.Kunde.Intern && b.Vorgang.Projekt.Bezeichnung != "Abwesenheiten")
                    .Select(b => b.Zeit)
                    .SumAsync() ?? decimal.Zero;

                var totalBuchungen = extBuchungen + intBuchungen;
                var extProzent = totalBuchungen > 0 ? (extBuchungen / totalBuchungen) * 100 : 0;
                var intProzent = totalBuchungen > 0 ? (intBuchungen / totalBuchungen) * 100 : 0;

                var extPerformance = new Performance
                {
                    Name = "Extern",
                    Wert = extBuchungen,
                    Total = totalBuchungen,
                    Prozent = extProzent,
                    Class = "bg-primary"
                };

                var intPerformance = new Performance
                {
                    Name = "Intern",
                    Wert = intBuchungen,
                    Total = totalBuchungen,
                    Prozent = intProzent,
                    Class = "bg-success"
                };

                performanceList.Add(new PerformanceList
                {
                    Name = team.Bezeichnung,
                    Performances = new List<Performance> { extPerformance, intPerformance }
                });
            }

            return performanceList;
        }

        public async Task<List<FrontendtestOverview>> GetFrontendtestOverviewsAsync(int take)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tFrontendtests = await context.TFrontendtest
                .AsNoTracking()
                .OrderByDescending(f => f.StartDatum)
                .Take(take)
                .ToListAsync();
            return _mapper.Map<List<FrontendtestOverview>>(tFrontendtests);
        }

        public async Task<Frontendtest> GetFrontendtestDetailAsync(Guid id)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tFrontendtests = await context.TFrontendtest
                .Include(f => f.TFrontendtestDetail)
                .ThenInclude(f => f.TFrontendtestBild)
                .AsNoTracking()
                .SingleOrDefaultAsync(f => f.Id == id)
                ?? throw new InvalidDataException($"Frontendtest {id} nicht gefunden");
            return _mapper.Map<Frontendtest>(tFrontendtests);
        }

        public async Task DeleteFrontendtestsAsync(int take)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            context.Database.SetCommandTimeout(300);

            // IDs der zu behaltenden Datensätze
            var keepIds = await context.TFrontendtest
                .OrderByDescending(x => x.StartDatum)
                .Select(x => x.Id)
                .Take(take)
                .ToListAsync();

            // IDs der zu löschenden Datensätze
            var idsToDelete = await context.TFrontendtest
                .Where(f => !keepIds.Contains(f.Id))
                .Select(f => f.Id)
                .ToHashSetAsync();

            if (idsToDelete.Count == 0)
                return; // Nichts zu tun

            var idsToDelete2 = await context.TFrontendtestDetail
                .Where(f => idsToDelete.Contains(f.FrontendtestId))
                .Select(f => f.Id)
                .ToHashSetAsync();

            var idsToDelete3 = await context.TFrontendtestBild
                .Where(f => idsToDelete2.Contains(f.FrontendtestDetailId))
                .Select(f => f.Id)
                .ToHashSetAsync();

            // Lösche TFrontendtestBild
            await context.TFrontendtestBild.Where(f => idsToDelete3.Contains(f.Id)).ExecuteDeleteAsync();
            // Lösche TFrontendtestDetail
            await context.TFrontendtestDetail.Where(f => idsToDelete2.Contains(f.Id)).ExecuteDeleteAsync();
            // Lösche TFrontendtest
            await context.TFrontendtest.Where(f => idsToDelete.Contains(f.Id)).ExecuteDeleteAsync();
        }
    }

}
