using ItreeNet.Data.Models;

namespace ItreeNet.Interfaces
{
    public interface IDashboardService
    {
        Task<decimal> GetFerienSaldoByMitarbeiterAsnyc(Guid mitarbeiterId);
        Task<decimal> GetFerienAktuellesSaldoByMitarbeiterAsync(Guid mitarbeiterId);
        Task<decimal> GetStundenSaldoByMitarbeiterAsync(Guid mitarbeiterId);
        Task<DashboardAktuellesSaldo?> GetStundenAktuellesSaldoByMitarbeiterAsync(Guid mitarbeiterId);
        Task<bool> IsAbschlussDoneAsync(Guid mitarbeiterId);
        Task<List<DashboardMitarbeiter>> GetDashboardMitarbeiterAsync();
        Task<List<Buchung>> TopBuchungenAsync(Guid mitarbeiterId);
        Task<List<Buchung>> GetProvisorischeBuchungenAsync(Guid mitarbeiterId);
        Task<List<DashboardProjekt>> GetProjectBookingsAsync();
        Task<List<PipelineRuns>> GetPipelineRunsAsync();
        Task<PerformanceList> GetProductivity(Guid mitarbeiterId, int jahr = 0);
        Task<List<PerformanceList>> GetTeamProductivity(Guid mitarbeiterId, int jahr = 0);
        Task<List<FrontendtestOverview>> GetFrontendtestOverviewsAsync(int take);
        Task<Frontendtest> GetFrontendtestDetailAsync(Guid id);
        Task DeleteFrontendtestsAsync(int take);
    }
}
