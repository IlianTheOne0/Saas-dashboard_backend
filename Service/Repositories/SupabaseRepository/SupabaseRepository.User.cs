namespace Service.Repositories;

using Service.Interfaces;
using Service.Models;
using Utils;

public partial class SupabaseRepository : ISupabaseRepositoryUser
{
    private const string TAG_USER = "SUPABASE-REPO-AUTH";

    public async Task<PersonalDataResultDto?> FetchProfile(string accessToken, string? contactId)
    {
        Logger.Debug(TAG_USER, $"Starting FetchProfile for access token: {accessToken}");

        try
        {
            if (string.IsNullOrEmpty(accessToken)) { Logger.Warn(TAG_USER, "Access token is empty."); return null; }

            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token or User not found in Auth."); return null; }
            
            Logger.Debug(TAG_USER, $"User Authenticated. ID: {userResponse.Id}");

            var targetId = !string.IsNullOrEmpty(contactId) ? contactId : userResponse.Id;

            var profileResponse = await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Where(profile => profile.FId == targetId).Single();
            if (profileResponse == null) { Logger.Warn(TAG_USER, "User exists in Auth but no Profile found in database."); return null; }

            Logger.Info(TAG_USER, "Profile fetched successfully.");

            return new PersonalDataResultDto { Id = profileResponse.FId, Name = profileResponse.Name, AvatarUrl = profileResponse.AvatarUrl, Role = profileResponse.Role, IsOnline = profileResponse.IsOnline };
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Fetching the user's profile failed: {error.Message}"); throw; }
    }

    public async Task<MSupabaseGroups?> FetchNonFavContacts(string accessToken)
    {
        Logger.Debug(TAG_USER, $"Starting FetchNonFavContacts for access token: {accessToken}");

        try
        {
            if (string.IsNullOrEmpty(accessToken)) { Logger.Warn(TAG_USER, "Access token is empty."); return null; }

            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token or User not found in Auth."); return null; }

            string userId = userResponse.Id;

            Logger.Debug(TAG_USER, $"User Authenticated. ID: {userId}");

            MSupabaseProfile? profileResponse = await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Where(profiles => profiles.FId == userId).Single();
            MSupabaseGroups? contactsResponse = await SupabaseConnection.SupabaseClient.From<MSupabaseGroups>().Where(groups => groups.Id == profileResponse.FGroupId).Single();
            if (contactsResponse == null) { Logger.Warn(TAG_USER, "User exists in Auth but no Profile > Groups found in database."); return null; }

            Logger.Info(TAG_USER, "Contacts fetched successfully.");

            return contactsResponse;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Fetching the user's contacts failed: {error.Message}"); throw; }
    }

    public async Task<MSupabaseContacts?> FetchFavContacts(string accessToken)
    {
        Logger.Debug(TAG_USER, $"Starting FetchFavContacts for access token: {accessToken}");

        try
        {
            if (string.IsNullOrEmpty(accessToken)) { Logger.Warn(TAG_USER, "Access token is empty."); return null; }

            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token or User not found in Auth."); return null; }

            string userId = userResponse.Id;

            Logger.Debug(TAG_USER, $"User Authenticated. ID: {userId}");

            MSupabaseContacts contactsResponse = await SupabaseConnection.SupabaseClient.From<MSupabaseContacts>().Where(contacts => contacts.FId == userId).Single();
            if (contactsResponse == null) { Logger.Warn(TAG_USER, "User exists in Auth but no Profile > Contacts found in database."); return null; }

            Logger.Info(TAG_USER, "Contacts fetched successfully.");

            return contactsResponse;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Fetching the user's contacts failed: {error.Message}"); throw; }
    }

    public async Task<bool> UpdateFavContacts(string accessToken, List<string> contacts)
    {
        Logger.Debug(TAG_USER, "Updating Favourite Contacts list...");

        try
        {
            if (string.IsNullOrEmpty(accessToken)) { Logger.Warn(TAG_USER, "Access token is empty."); return false; }

            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token."); return false; }

            string userId = userResponse.Id; var updateModel = new MSupabaseContacts { FId = userId, Contacts = contacts };
            var response = await SupabaseConnection.SupabaseClient.From<MSupabaseContacts>().Upsert(updateModel);

            Logger.Info(TAG_USER, "Contacts list updated successfully in DB.");
            return true;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Failed to update contacts: {error.Message}"); throw; }
    }
}