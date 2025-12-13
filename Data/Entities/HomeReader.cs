namespace Data.Entities;

using Data.Entities.Interfaces;
using Data.Entities.Abstracts;

public class HomeReader : AReader, IHomeReader
{
    public HomeReader() : base("Home/") { }

    public Task<string> GetDashboardData() => Task.Run(() => Read("dashboard"));
    public Task<string> GetRealtimeData() => Task.Run(() => Read("realtime"));
    public Task<string> GetAudienceData() => Task.Run(() => Read("audience"));
    public Task<string> GetTrafficSourceData() => Task.Run(() => Read("traffic_source"));
}