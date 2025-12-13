namespace Service.Repositories;

using Service.Interfaces;
using Service.Models;
using Supabase.Gotrue;
using Utils;

public partial class SupabaseRepository : ISupabaseRepositoryAuth
{
    private const string TAG_AUTH = "SUPABASE-REPO-AUTH";

    public async Task<bool> RegisterUser(string email, string password, string name)
    {
        string? createdUserId = null;
        Logger.Debug(TAG_AUTH, $"Starting RegisterUser for: {email}");

        try
        {
            if (await SupabaseConnection!.IsUserExists(email)) { Logger.Warn(TAG_AUTH, $"User with email {email} already exists."); throw new Exception("User already exists"); }

            var response = await SupabaseConnection!.SupabaseClient.Auth.SignUp(email, password);

            if (response == null) { throw new Exception("No response from Supabase"); }
            if (response.User == null) { throw new Exception("User registration failed"); }
            if (string.IsNullOrEmpty(response.User.Id)) throw new Exception("User ID not returned");

            createdUserId = response.User.Id;
            Logger.Debug(TAG_AUTH, $"Auth User Created. ID: {createdUserId}");

            MSupabaseGroups? group = await SupabaseConnection!.SupabaseClient.From<MSupabaseGroups>().Single();
            MSupabaseProfile newProfile = new MSupabaseProfile { FId = createdUserId, Name = name, Username = $"{name}#{createdUserId.Trim().Split("-")[0]}", FGroupId = group!.Id };
            await SupabaseConnection!.SupabaseClient.From<MSupabaseProfile>().Insert(newProfile);
            Logger.Debug(TAG_AUTH, $"Profile was created!");

            if (group.ProfileIds == null) { group.ProfileIds = new List<string>(); }
            group.ProfileIds!.Add(createdUserId);
            await SupabaseConnection!.SupabaseClient.From<MSupabaseGroups>().Upsert(group);
            Logger.Debug(TAG_AUTH, $"Profile in the group was added");
            
            await SupabaseConnection!.SupabaseClient.From<MSupabaseContacts>().Insert(new MSupabaseContacts { FId = createdUserId });
            Logger.Debug(TAG_AUTH, $"Profile's contacts was added");

            Logger.Info(TAG_AUTH, "Profile inserted successfully.");
            return true;
        }
        catch (Exception error)
        {
            Logger.Error(TAG_AUTH, $"Registration failed: {error.Message}");

            if (!string.IsNullOrEmpty(createdUserId))
            {
                Logger.Warn(TAG_AUTH, $"[ROLLBACK] Attempting to delete zombie user: {createdUserId}");
                try
                {
                    bool response = await SupabaseConnection!.DeleteUser(createdUserId);
                    if (!response) { Logger.Error(TAG_AUTH, $"[ROLLBACK FAILED] Could not delete user {createdUserId}"); }
                    else { Logger.Info(TAG_AUTH, "[ROLLBACK] User deleted successfully"); }
                }
                catch (Exception deletionError) { Logger.Error(TAG_AUTH, $"[ROLLBACK EXCEPTION] Could not delete user {createdUserId}", deletionError); }
            }

            throw new Exception($"Registration failed and was rolled back: {error.Message}");
        }
    }
    public async Task<string?> LoginUser(string email, string password)
    {
        try
        {
            Logger.Debug(TAG_AUTH, $"Attempting Login for: {email}");

            Session? session = await SupabaseConnection!.SupabaseClient.Auth.SignIn(email, password);

            if (session != null && session.User != null) { Logger.Debug(TAG_AUTH, $"Login Result: success!"); return session.AccessToken; }

            Logger.Debug(TAG_AUTH, $"Login Result: failed!"); 
            return null;
        }
        catch (Exception error) { throw new Exception($"Failed to login the user: {error.Message}", error); }
    }

    public async Task<bool> RecoveryPassword(string email)
    {
        try
        {
            Logger.Debug(TAG_AUTH, $"Initiating Password Reset for: {email}");
            var response = await SupabaseConnection!.SupabaseClient.Auth.ResetPasswordForEmail(email);
            return response;
        }
        catch (Exception error) { throw new Exception($"Failed to recover the password: {error.Message}", error); }
    }

    public async Task<bool> SetNewPassword(string password, string accessToken, string refreshToken)
    {
        try
        {
            Logger.Debug(TAG_AUTH, $"Initiating New Password for");

            await SupabaseConnection!.SupabaseClient.Auth.SetSession(accessToken, refreshToken);
            await SupabaseConnection!.SupabaseClient.Auth.Update(new UserAttributes { Password = password });
            await SupabaseConnection!.SupabaseClient.Auth.SignOut();

            return true;
        }
        catch (Exception error) { throw new Exception($"Failed to set the new password: {error.Message}", error); }
    }

    public async Task<bool> UpdateEmail(string accessToken, string newEmail)
    {
        try
        {
            Logger.Debug(TAG_AUTH, $"Updating email to: {newEmail}");
            var attrs = new UserAttributes { Email = newEmail };
            var response = await SupabaseConnection!.SupabaseClient.Auth.Update(attrs);
            return response != null;
        }
        catch (Exception error) { throw new Exception($"Failed to update email: {error.Message}", error); }
    }

    public async Task<bool> ChangePassword(string accessToken, string newPassword)
    {
        try
        {
            Logger.Debug(TAG_AUTH, "Changing password for authenticated user");
            var attrs = new UserAttributes { Password = newPassword };
            var response = await SupabaseConnection!.SupabaseClient.Auth.Update(attrs);
            return response != null;
        }
        catch (Exception error) { throw new Exception($"Failed to change password: {error.Message}", error); }
    }
}