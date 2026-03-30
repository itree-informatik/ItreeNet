using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IBuchungsService
    {
        Task<List<Buchungstag>> GetBookingsByEmployeeAsync(Guid id, DateOnly date, bool isWeek);
        Task<List<Buchungstag>> InsertBuchungAsync(Buchung booking, bool isWeek);
        Task<List<Buchungstag>> DeleteBuchungAsync(Buchung booking, bool isWeek);
        Task<List<Buchungstag>> UpdateBuchungAsync(Buchung booking, bool isWeek);
        Task MonatsAbschlussAsync(int year, int month);
        Task MonatsAbschlussAdminAsync(Guid mitarbeiterId, int year, int month);
        Task<List<Buchungstag>> InsertBuchungenAsync(Buchung booking, bool isWeek, DateOnly dateTo);
        Task<List<Buchung>> SucheBuchungAsync(Guid? selectedMitarbeiter, Guid? selectedTeam, Guid? selectedKunde,
            Guid? selectedProjekt, Guid? selectedAktivitaet, DateOnly? from, DateOnly? to);
        Task SetAbgerechnetAsync();
        Task SetNichtProvisorischAsync(List<Guid> buchungIds);
    }
}


