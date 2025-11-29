namespace Service.Repositories;

using Service.DataSources;
using Service.Interfaces;
using Service.Models;
using Utils;

public partial class SupabaseRepository : ADatabaseRepository, ISupabaseRepository
{
    private const string TAG_MAIN = "SUPABASE-REPO";
    public SupabaseDataSource? SupabaseConnection { get; set; } = null;

    public async Task InitDatabase()
    {
        try
        {
            const string filePath = "../.config/supabase_keys.json";
            Logger.Debug(TAG_MAIN, $"Reading config from: {filePath}");

            MCSupabasse supabaseConfig = await _initDatabase<MCSupabasse>(filePath);
            if (supabaseConfig == null || supabaseConfig.Key == null || supabaseConfig.Url == null) { throw new Exception("Cannot read conifg!"); }

            SupabaseConnection = new SupabaseDataSource(supabaseConfig.Url, supabaseConfig.Key);

            await SupabaseConnection.Initialize();
            Logger.Info(TAG_MAIN, "Repository Initialized.");
        }
        catch (Exception error) { throw new Exception("Error initializing Supabase client!", error); }
    }
}