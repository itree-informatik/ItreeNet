namespace ItreeNet.Interfaces
{
    public interface IPensumService
    {
        Task<decimal> GetWeeklyWorkloadByEmployeeAsync(Guid id, DateOnly date);
        Task<decimal> GetMonthlyWorkloadByEmployeeAsync(Guid id, DateOnly date);
        Task<decimal> GetDailyWorkloadByemployeeAsync(Guid id, DateOnly date);
    }
}