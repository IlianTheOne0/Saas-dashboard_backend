namespace Data.Entities.Interfaces;

public interface IHomeReader
{
    Task<string> GetDashboardData();
    Task<string> GetRealtimeData();
    Task<string> GetAudienceData();
    Task<string> GetTrafficSourceData();
}