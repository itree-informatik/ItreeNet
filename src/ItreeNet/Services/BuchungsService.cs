using AutoMapper;
using ItreeNet.Data.Enums;
using ItreeNet.Data.Extensions;
using ItreeNet.Data.Models;
using ItreeNet.Data.Models.DB;
using ItreeNet.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security;

namespace ItreeNet.Services
{
    public class BuchungsService : IBuchungsService
    {
        private readonly IDbContextFactory<ZeiterfassungContext> _dbFactory;
        private readonly IMapper _mapper;
        private readonly IVorgangService _vorgangService;
        private readonly IPensumService _pensumService;
        private readonly UserService _userService;
        private readonly IMitarbeiterService _mitarbeiterService;
        private readonly IMailService _mailService;
        private readonly IProjektService _projektService;

        public BuchungsService(IDbContextFactory<ZeiterfassungContext> dbFactory, IMapper mapper, IVorgangService vorgangService, UserService userService, 
            IPensumService pensumService, IMitarbeiterService mitarbeiterService, IMailService mailService, IProjektService projektService)
        {
            _dbFactory = dbFactory;
            _mapper = mapper;
            _vorgangService = vorgangService;
            _userService = userService;
            _pensumService = pensumService;
            _mitarbeiterService = mitarbeiterService;
            _mailService = mailService;
            _projektService = projektService;
        }

        public async Task<List<Buchungstag>> GetBookingsByEmployeeAsync(Guid id, DateOnly date, bool isWeek)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            // security
            var currentUser = _userService.CurrentUser;
            if (currentUser == null)
            {
                throw new Exception("CurrentUser is null");
            }
            else
            {
                if (!currentUser.IsAdmin && currentUser.MitarbeiterId != id)
                {
                    throw new Exception("Du bist nicht berechtigt");
                }
            }

            DateOnly startDate;
            DateOnly endDate;
            int days = isWeek ? 7 : DateTime.DaysInMonth(date.Year, date.Month);

            if (isWeek)
            {
                startDate = GetWeekToDisplay(date);
                endDate = startDate.AddDays(7);
            }
            else
            {
                startDate = new DateOnly(date.Year, date.Month, 1);
                endDate = startDate.AddMonths(1).AddDays(-1);
            }

            var tBookings = await context.TBuchung
                                    .AsNoTracking()
                                    .Include(x => x.Vorgang)
                                    .ThenInclude(x => x.Projekt)
                                    .ThenInclude(x => x.Kunde)
                                    .Include(x => x.ChangedByNavigation)
                                    .Where(x => x.MitarbeiterId == id && startDate <= x.Datum && x.Datum <= endDate)
                                    .OrderBy(x => x.Datum)
                                    .ToListAsync();

            if (tBookings == null)
            {
                throw new InvalidDataException("No bookings for employee found");
            }
            var dateTimeFormat = new CultureInfo("de-DE").DateTimeFormat;

            var buchungstage = Enumerable.Range(0, days).Select(i => new Buchungstag(_dbFactory)
            {
                Weekday = startDate.AddDays(i).ToString("dddd", dateTimeFormat),
                Date = startDate.AddDays(i),
            }).ToList();

            await AddInformationToBuchung(buchungstage, tBookings);

            return buchungstage;
        }

        public async Task<List<Buchungstag>> UpdateBuchungAsync(Buchung booking, bool isWeek)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            booking = await ChangedByUserAsync(booking);
            var tBuchung = _mapper.Map<TBuchung>(booking);

            context.Entry(tBuchung).State = EntityState.Modified;
            await context.SaveChangesAsync();
            await SendMailAsync(booking.VorgangId);

            return await GetBookingsByEmployeeAsync(booking.MitarbeiterId, booking.Datum, isWeek);
        }

        public async Task MonatsAbschlussAsync(int year, int month)
        {
            if (_userService.CurrentUser == null || _userService.CurrentUser.MitarbeiterId == null || _userService.CurrentUser.MitarbeiterId == Guid.Empty)
            {
                throw new InvalidDataException("CurrentUser nicht gefunden");
            }

            var mitarbeiterId = (Guid)_userService.CurrentUser.MitarbeiterId;

            await CreateMonatsAbschluss(mitarbeiterId, year, month);
        }

        public async Task MonatsAbschlussAdminAsync(Guid mitarbeiterId, int year, int month)
        {
            if (_userService.CurrentUser == null || _userService.CurrentUser.MitarbeiterId == null || _userService.CurrentUser.MitarbeiterId == Guid.Empty)
            {
                throw new InvalidDataException("CurrentUser nicht gefunden");
            }

            var currentUserId = _userService.CurrentUser.Uid;

            if (!Globals.BossList.Contains(currentUserId.ToString(), StringComparer.OrdinalIgnoreCase))
                throw new SecurityException(
                    "Sie haben keine Berechtigungen für einen andere Mitarbeiter den Abschluss durchzuführen");

            await CreateMonatsAbschluss(mitarbeiterId, year, month);
        }

        public async Task<List<Buchungstag>> InsertBuchungAsync(Buchung booking, bool isWeek)
        {
            await SaveAsync(booking);
            return await GetBookingsByEmployeeAsync(booking.MitarbeiterId, booking.Datum, isWeek);
        }

        public async Task<List<Buchungstag>> InsertBuchungenAsync(Buchung booking, bool isWeek, DateOnly dateTo)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var reduktionenDaten = await context.TArbeitszeitReduktion
                .AsNoTracking()
                .Where(r => r.Datum >= booking.Datum && r.Datum <= dateTo)
                .Select(r => r.Datum)
                .ToHashSetAsync();

            for (var day = booking.Datum; day <= dateTo; day = day.AddDays(1))
            {
                if (!reduktionenDaten.Contains(day) && day.DayOfWeek != DayOfWeek.Saturday && day.DayOfWeek != DayOfWeek.Sunday)
                {
                    var newBooking = booking.Clone();
                    newBooking!.Datum = day;
                    await SaveAsync(newBooking);
                }
            }

            return await GetBookingsByEmployeeAsync(booking.MitarbeiterId, booking.Datum, isWeek);
        }

        public async Task<List<Buchungstag>> DeleteBuchungAsync(Buchung booking, bool isWeek)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var tBuchung = _mapper.Map<TBuchung>(booking);
            context.TBuchung.Remove(tBuchung);
            await context.SaveChangesAsync();

            await SendMailAsync(booking.VorgangId);

            return await GetBookingsByEmployeeAsync(booking.MitarbeiterId, booking.Datum, isWeek);
        }

        public async Task<List<Buchung>> SucheBuchungAsync(Guid? selectedMitarbeiter, Guid? selectedTeam, Guid? selectedKunde, Guid? selectedProjekt, Guid? selectedAktivitaet, DateOnly? from, DateOnly? to)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            IQueryable<TBuchung> query = context.Set<TBuchung>()
                .AsNoTracking()
                .Include(b => b.Vorgang).ThenInclude(v => v.Projekt).ThenInclude(p => p.Kunde)
                .Include(b => b.Mitarbeiter);

            if (selectedMitarbeiter != null)
                query = query.Where(m => m.MitarbeiterId == selectedMitarbeiter);

            if (selectedTeam != null)
            {
                var teamMitarbeiter = await context.TMitarbeiterTeam
                    .Where(t => t.TeamId == selectedTeam)
                    .Select(m => m.MitarbeiterId)
                    .ToListAsync();
                query = query.Where(m => teamMitarbeiter.Contains(m.MitarbeiterId));
            }

            if (selectedKunde != null)
                query = query.Where(m => m.Vorgang.Projekt.KundeId == selectedKunde);

            if (selectedProjekt != null)
                query = query.Where(m => m.Vorgang.ProjektId == selectedProjekt);

            if (selectedAktivitaet != null)
                query = query.Where(m => m.VorgangId == selectedAktivitaet);

            if (from != null)
                query = query.Where(m => m.Datum >= from);

            if (to != null)
                query = query.Where(m => m.Datum <= to);

            var tResult = await query.Take(1000).OrderByDescending(d => d.Datum).ToListAsync();

            var result = _mapper.Map<List<Buchung>>(tResult);

            for (var i = 0; i < result.Count; i++)
            {
                var t = tResult[i];
                result[i].Vorgang = _mapper.Map<Vorgang>(t.Vorgang);
                result[i].KundenName = t.Vorgang?.Projekt?.Kunde?.Kundenname;
                result[i].ProjektName = t.Vorgang?.Projekt?.Bezeichnung;
                result[i].Mitarbeiter = _mapper.Map<Mitarbeiter>(t.Mitarbeiter);
            }

            return result;
        }

        private async Task CreateMonatsAbschluss(Guid mitarbeiterId, int year, int month)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();

            var firstLoop = true;
            var firstSaldo = await context.TMitarbeiterSaldo
                .Where(m => m.MitarbeiterId == mitarbeiterId && m.Jahr * 100 + m.Monat >= year * 100 + month)
                .OrderBy(m => m.Jahr).ThenBy(m => m.Monat)
                .FirstOrDefaultAsync() ?? await context.TMitarbeiterSaldo
                .Where(m => m.MitarbeiterId == mitarbeiterId)
                .OrderBy(m => m.Jahr).ThenBy(m => m.Monat)
                .LastOrDefaultAsync();
            if (firstSaldo == null)
            {
                throw new Exception($"Für den Mitarbeiter '{_userService.CurrentUser!.Name}' gibt es noch keinen initialen Mitarbeitersaldo.");
            }

            var firstSaldoDate = new DateTime(firstSaldo.Jahr, firstSaldo.Monat, 1);
            var jahrvon = firstSaldoDate.Year;
            var monatvon = firstSaldoDate.Month;
            var jahrbis = DateTime.Today.Month == 1 ? DateTime.Today.Year -1 : DateTime.Today.Year;
            var monatbis = DateTime.Today.Month == 1 ? 12 : DateTime.Today.Month - 1;

            for (var yearForCalc = jahrvon; yearForCalc <= jahrbis; yearForCalc++)
            {
                var monthsToLoop = 12;
                if (yearForCalc == jahrbis)
                {
                    monthsToLoop = monatbis;
                }

                for (var monthForCalc = firstLoop ? monatvon : 1; monthForCalc <= monthsToLoop; monthForCalc++)
                {
                    await CalcMonthAsync(mitarbeiterId, yearForCalc, monthForCalc);
                    firstLoop = false;
                }
            }
        }

        private static DateOnly GetWeekToDisplay(DateOnly date)
        {
            var startOfWeek = date.AddDays(
                  (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek -
                  (int)date.DayOfWeek);

            return startOfWeek;
        }

        private async Task AddInformationToBuchung(List<Buchungstag> buchungstage, List<TBuchung> tBookings)
        {
            foreach (var buchungstag in buchungstage)
            {
                var tBuchungList = tBookings.Where(d => d.Datum == buchungstag.Date).ToList();
                var buchungListe = _mapper.Map<List<Buchung>>(tBuchungList);

                for (var i = 0; i < buchungListe.Count; i++)
                {
                    var buchung = buchungListe[i];
                    var tBuchung = tBuchungList[i];

                    // Daten sind bereits via Include geladen — kein zusätzlicher DB-Aufruf nötig
                    buchung.KundenName = tBuchung.Vorgang?.Projekt?.Kunde?.Kundenname;
                    buchung.ProjektName = tBuchung.Vorgang?.Projekt?.Bezeichnung;

                    if (buchung.ChangedBy != null && buchung.ChangedBy != Guid.Empty && tBuchung.ChangedByNavigation != null)
                    {
                        buchung.ChangedByMitarbeiter = _mapper.Map<Mitarbeiter>(tBuchung.ChangedByNavigation);
                    }

                    if (buchung.OriginalVorgangId != null && buchung.OriginalVorgangId != Guid.Empty)
                    {
                        buchung.OriginalVorgang = await _vorgangService.GetAsync((Guid)buchung.OriginalVorgangId);
                    }
                }

                buchungstag.Details = new List<Buchung>(buchungListe);
            }
        }

        private async Task SaveAsync(Buchung model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            model = await ChangedByUserAsync(model);
            model.Id = Guid.NewGuid();
            var tBuchung = _mapper.Map<TBuchung>(model);
            context.TBuchung.Add(tBuchung);
            await context.SaveChangesAsync();
            await SendMailAsync(model.VorgangId);
        }

        private async Task SendMailAsync(Guid vorgangId)
        {
            var project = await _vorgangService.GetProjektFromVorgangIdAsync(vorgangId);
            var vorgaenge = await _vorgangService.GetAllProjecIdAsync(project.Id);

            var anzahlStundenTotal = vorgaenge.Sum(v => v.AnzahlStunden);
            var gebuchteStundenTotal = vorgaenge.Sum(v => v.GebuchteStunden);

            if (anzahlStundenTotal > 0 && gebuchteStundenTotal > 0)
            {
                var prozent = Math.Round((gebuchteStundenTotal / anzahlStundenTotal) * 100, 0);

                if (prozent >= 80)
                {
                    await _mailService.SendBookingNotificationAsync(project, prozent);
                }
                else
                {
                    if (project.EmailGesendet80 || project.EmailGesendet90 || project.EmailGesendet100)
                    {
                        project.EmailGesendet80 = false;
                        project.EmailGesendet90 = false;
                        project.EmailGesendet100 = false;

                        await _projektService.SaveSingleAsync(project);
                    }
                }
            }
        }

        private async Task<Buchung> ChangedByUserAsync(Buchung model)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            var mitarbeiter = await context.TMitarbeiter
                                    .AsNoTracking()
                                    .SingleOrDefaultAsync(m => m.Id == model.MitarbeiterId)
                                ?? throw new InvalidDataException($"Mitarbeiter {model.MitarbeiterId} nicht gefunden");

            if (mitarbeiter.AzureId != _userService.CurrentUser!.Uid)
            {
                if (model.Id != Guid.Empty)
                {
                    var oldModel = await context.TBuchung.AsNoTracking().SingleOrDefaultAsync(b => b.Id == model.Id)
                        ?? throw new InvalidDataException($"Buchung {model.Id} nicht gefunden");

                    model.ChangedOn = DateTime.Now;

                    if (model.ChangedBy != _userService.CurrentUser.MitarbeiterId)
                    {
                        model.ChangedBy = _userService.CurrentUser.MitarbeiterId;

                        if (model.OriginalVorgangId == null && model.VorgangId != oldModel.VorgangId)
                            model.OriginalVorgangId = oldModel.VorgangId;

                        if (model.OriginalDatum == null && model.Datum != oldModel.Datum)
                            model.OriginalDatum = oldModel.Datum;

                        if (model.OriginalZeit == null && model.Zeit != oldModel.Zeit)
                            model.OriginalZeit = oldModel.Zeit;

                        if (model.OriginalText == null && model.Buchungstext != oldModel.Buchungstext)
                            model.OriginalText = oldModel.Buchungstext;
                    }

                }
                else
                {
                    model.ChangedBy = _userService.CurrentUser.MitarbeiterId;
                    model.ChangedOn = DateTime.Now;
                }
            }
            else
            {
                model.ChangedBy = null;
                model.ChangedOn = null;
            }

            return model;
        }

        private async Task CalcMonthAsync(Guid mitarbeiterId, int year, int month)
        {
            await using var context = await _dbFactory.CreateDbContextAsync();
            TMitarbeiterSaldo previousMonat;
            if (month == 1)
            {
                previousMonat = await context.TMitarbeiterSaldo
                                    .SingleOrDefaultAsync(m =>
                                        m.MitarbeiterId == mitarbeiterId &&
                                        m.Jahr == year - 1 && m.Monat == 12)
                                ?? throw new InvalidDataException($"Kein Mitarbeiter-Saldo für {mitarbeiterId} ({year - 1}/12) gefunden");
            }
            else
            {
                previousMonat = await context.TMitarbeiterSaldo
                                    .SingleOrDefaultAsync(m =>
                                        m.MitarbeiterId == mitarbeiterId &&
                                        m.Jahr == year &&
                                        m.Monat == month - 1)
                                ?? throw new InvalidDataException($"Kein Mitarbeiter-Saldo für {mitarbeiterId} ({year}/{month - 1}) gefunden");
            }

            // Anwesenheitsdaten für den Monat laden (statt Buchungen)
            var monatsStart = new DateOnly(year, month, 1);
            var monatsEnde = monatsStart.AddMonths(1).AddDays(-1);

            var anwesenheiten = await context.TAnwesenheit
                .AsNoTracking()
                .Where(a => a.MitarbeiterId == mitarbeiterId && a.Datum >= monatsStart && a.Datum <= monatsEnde)
                .ToListAsync();

            var mitarbeiterSaldo = new TMitarbeiterSaldo
            {
                Jahr = year,
                Monat = month,
                MitarbeiterId = mitarbeiterId,
                Id = Guid.NewGuid()
            };

            var saldoAlreadyExists = await context.TMitarbeiterSaldo.SingleOrDefaultAsync(m => m.MitarbeiterId == mitarbeiterId && m.Jahr == year && m.Monat == month);

            if (saldoAlreadyExists != null)
            {
                mitarbeiterSaldo = saldoAlreadyExists;
            }

            // Ferien aus T_Anwesenheit mit Typ 'Ferien'
            var ferienUsed = anwesenheiten
                .Where(a => a.Typ == EnumAnwesenheitTyp.Ferien)
                .Sum(a => a.Zeit ?? 0m);

            decimal ferienSaldo;
            if (month == 1)
            {
                var ferienSaldoForYear = await context.TFerienArbeitspensum
                    .AsNoTracking()
                    .Where(f => f.MitarbeiterId == mitarbeiterId && f.GueltigAb <= new DateOnly(year, 1, 1))
                    .OrderByDescending(f => f.GueltigAb)
                    .Select(f => f.FerienProJahr)
                    .FirstOrDefaultAsync();

                ferienSaldo = previousMonat.FerienSaldo + ferienSaldoForYear;
            }
            else
            {
                ferienSaldo = previousMonat.FerienSaldo;
            }

            if (ferienUsed != 0)
            {
                var stundenProTag = await _pensumService.GetDailyWorkloadByemployeeAsync(mitarbeiterId, new DateOnly(year, month, 1));
                mitarbeiterSaldo.FerienSaldo = stundenProTag > 0
                    ? ferienSaldo - (ferienUsed / stundenProTag)
                    : ferienSaldo;
            }
            else
            {
                mitarbeiterSaldo.FerienSaldo = ferienSaldo;
            }

            // korrekturen — einmalig laden, für Ferien und Stunden verwenden
            var korrektur = await context.TMitarbeiterSaldoKorrektur.SingleOrDefaultAsync(k =>
                k.MitarbeiterId == mitarbeiterId && k.Jahr == year && k.Monat == month);

            if (korrektur?.Ferien != null)
                mitarbeiterSaldo.FerienSaldo += (decimal)korrektur.Ferien;

            // Ist-Stunden: Anwesenheit + Ferien (exkl. Gleitzeit, wie bisher)
            decimal saldoIst = anwesenheiten
                .Where(a => a.Typ != EnumAnwesenheitTyp.Gleitzeit)
                .Sum(a => a.Zeit ?? 0m);

            if (korrektur?.Stunden != null)
                saldoIst += (decimal)korrektur.Stunden;

            mitarbeiterSaldo.Ist = saldoIst;
            mitarbeiterSaldo.Soll = await _pensumService.GetMonthlyWorkloadByEmployeeAsync(mitarbeiterId, new DateOnly(year, month, 1));
            mitarbeiterSaldo.StundenSaldo = previousMonat.StundenSaldo + (saldoIst - mitarbeiterSaldo.Soll);

            if (saldoAlreadyExists != null)
            {
                context.Entry(mitarbeiterSaldo).State = EntityState.Modified;
            }
            else
            {
                context.TMitarbeiterSaldo.Add(mitarbeiterSaldo);
            }

            await context.SaveChangesAsync();
        }
    }
}
