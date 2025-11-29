namespace Service.DataSources;

using Service.Interfaces;
using Supabase;

using Utils;

public class SupabaseDataSource : ISupabaseDataSource
{
    private const string TAG = "SUPABASE-DS";

    private string _key;
    public Client SupabaseClient { get; private set; } = null!;

    public SupabaseDataSource(string Url, string Key)
    {
        SupabaseOptions options = new SupabaseOptions { AutoConnectRealtime = true, AutoRefreshToken = false };
        SupabaseClient = new Client(Url, Key, options);

        _key = Key;
    }

    public async Task Initialize()
    {
        await SupabaseClient.InitializeAsync();
        Logger.Info(TAG, "Supabase client initialized.");
    }

    public async Task<bool> DeleteUser(string userId)
    {
        Logger.Warn(TAG, $"Deleting the user with {userId} UserID");

        try { return await SupabaseClient.AdminAuth(_key).DeleteUser(userId); }
        catch (Exception error) { Logger.Error(TAG, $"Failed to delete user: {error.Message}"); return false; }
    }

    public async Task<bool> IsUserExists(string email)
    {
        Logger.Debug(TAG, $"Check, if the user with {email} email exists");

        try
        {
            var parameters = new Dictionary<string, object> { { "email_input", email } };
            var response = await SupabaseClient.Rpc("user_exists_by_email", parameters);
            
            if (response == null || response.Content == null) { throw new Exception("No response from Supabase"); }

            return bool.Parse(response.Content);
        }
        catch (Exception error) { Logger.Error(TAG, $"Error checking user existence: {error.Message}"); return false; }
    }   
}