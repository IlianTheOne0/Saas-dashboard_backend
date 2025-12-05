namespace Service.Repositories;

using Service.Interfaces;
using Service.Models;
using Utils;

public partial class SupabaseRepository : ISupabaseRepositoryUser
{
    private const string TAG_USER = "SUPABASE-REPO-AUTH";

    public async Task<PersonalDataResultDto?> FetchProfile(string accessToken)
    {
        Logger.Debug(TAG_USER, $"Starting FetchProfile for access token: {accessToken}");

        try
        {
            if (string.IsNullOrEmpty(accessToken)) { Logger.Warn(TAG_USER, "Access token is empty."); return null; }

            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token or User not found in Auth."); return null; }

            string userId = userResponse.Id;
            string userEmail = userResponse.Email ?? "Unknown";
            
            Logger.Debug(TAG_USER, $"User Authenticated. ID: {userId}");

            var profileResponse = await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Where(profile => profile.Id == userId).Single();
            if (profileResponse == null) { Logger.Warn(TAG_USER, "User exists in Auth but no Profile found in database."); return null; }

            Logger.Info(TAG_USER, "Profile fetched successfully.");

            return new PersonalDataResultDto { Id = userId, Email = userEmail, Name = profileResponse.Name, AvatarUrl = profileResponse.AvatarUrl };
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Fetching the user's profile failed: {error.Message}"); throw; }
    }
}