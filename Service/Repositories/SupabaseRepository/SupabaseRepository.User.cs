namespace Service.Repositories;

using Service.Interfaces;
using Service.Models;
using Utils;

public partial class SupabaseRepository : ISupabaseRepositoryUser
{
    private const string TAG_USER = "SUPABASE-REPO-USER";

    public async Task<PersonalDataResultDto?> FetchProfile(string accessToken, string? contactId)
    {
        Logger.Debug(TAG_USER, $"Starting FetchProfile for access token: {accessToken}");

        try
        {
            if (string.IsNullOrEmpty(accessToken)) { Logger.Warn(TAG_USER, "Access token is empty."); return null; }

            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token or User not found in Auth."); return null; }

            var targetId = !string.IsNullOrEmpty(contactId) ? contactId : userResponse.Id;

            var profileResponse = await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Where(profile => profile.FId == targetId).Single();
            if (profileResponse == null) { Logger.Warn(TAG_USER, "User exists in Auth but no Profile found in database."); return null; }

            return new PersonalDataResultDto
            {
                Id = profileResponse.FId,
                Name = profileResponse.Name,
                Email = (contactId == null) ? userResponse.Email : null,
                AvatarUrl = profileResponse.AvatarUrl,
                Role = profileResponse.Role,
                IsOnline = profileResponse.IsOnline,
                Location = profileResponse.Location,
                Timezone = profileResponse.Timezone,
                Phone = profileResponse.Phone
            };
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

    public async Task<bool> UpdateProfile(string accessToken, UpdateProfileDto data)
    {
        Logger.Debug(TAG_USER, "Updating User Profile...");
        try
        {
            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) throw new Exception("Invalid Access Token");

            var existingProfile = await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Where(user => user.FId == userResponse.Id).Single();

            if (existingProfile == null) { Logger.Warn(TAG_USER, "Profile not found for update."); return false; }

            if (data.Name != null) { existingProfile.Name = data.Name; }
            if (data.Role != null) { existingProfile.Role = data.Role; }
            if (data.Location != null) { existingProfile.Location = data.Location; }
            if (data.Timezone != null) { existingProfile.Timezone = data.Timezone; }
            if (data.Phone != null) { existingProfile.Phone = data.Phone; }
            if (data.AvatarUrl != null) { existingProfile.AvatarUrl = data.AvatarUrl; }

            await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Update(existingProfile);

            Logger.Info(TAG_USER, "Profile updated successfully.");
            return true;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Update profile failed: {error.Message}"); return false; }
    }

    public async Task<string?> UpdateAvatar(string accessToken, string base64Image, string fileName)
    {
        Logger.Debug(TAG_USER, "Uploading new avatar...");
        try
        {
            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { throw new Exception("Invalid Access Token"); }

            if (base64Image.Contains(",")) { base64Image = base64Image.Split(',')[1]; }
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            string ext = Path.GetExtension(fileName);
            if (string.IsNullOrEmpty(ext)) { ext = ".png"; }
            string storageFileName = $"{userResponse.Id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";

            string? publicUrl = await SupabaseConnection.UploadAvatar(imageBytes, storageFileName);
            if (publicUrl == null) { throw new Exception("Storage upload failed"); }

            var existingProfile = await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Where(profile => profile.FId == userResponse.Id).Single();

            if (existingProfile != null)
            {
                existingProfile.AvatarUrl = publicUrl;
                await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Update(existingProfile);
            }

            return publicUrl;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Avatar update failed: {error.Message}"); return null; }
    }

    public async Task<bool> DeleteAvatar(string accessToken)
    {
        Logger.Debug(TAG_USER, "Deleting avatar...");
        try
        {
            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { throw new Exception("Invalid Access Token"); }

            var existingProfile = await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Where(profile => profile.FId == userResponse.Id).Single();

            if (existingProfile != null)
            {
                existingProfile.AvatarUrl = null;
                await SupabaseConnection.SupabaseClient.From<MSupabaseProfile>().Update(existingProfile);
                return true;
            }
            return false;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Avatar deletion failed: {error.Message}"); return false; }
    }

    public async Task<List<MSupabaseEvent>> FetchCalendarEvents(string accessToken)
    {
        Logger.Debug(TAG_USER, "Fetching calendar events...");
        try
        {
            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { throw new Exception("Invalid Access Token"); }

            var response = await SupabaseConnection.SupabaseClient.From<MSupabaseEvent>().Where(_event => _event.UserId == userResponse.Id).Get();

            return response.Models;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Fetch Events failed: {error.Message}"); return new List<MSupabaseEvent>(); }
    }
    public async Task<bool> AddCalendarEvent(string accessToken, CalendarEventDto eventData)
    {
        Logger.Debug(TAG_USER, "Adding new calendar event...");
        try
        {
            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token."); return false; }

            var eventForCreator = new MSupabaseEvent
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userResponse.Id,
                Title = eventData.Title,
                Description = eventData.Description,
                StartTime = eventData.StartTime,
                EndTime = eventData.EndTime,
                Priority = eventData.Priority,
                Color = eventData.Color,
                Attendees = eventData.Attendees ?? new List<object>()
            };

            await SupabaseConnection.SupabaseClient.From<MSupabaseEvent>().Insert(eventForCreator);

            if (eventData.AttendeeIds != null && eventData.AttendeeIds.Count > 0)
            {
                Logger.Info(TAG_USER, $"Distributing event to {eventData.AttendeeIds.Count} attendees...");

                foreach (var attendeeId in eventData.AttendeeIds)
                {
                    if (attendeeId == userResponse.Id) { continue; }

                    var eventForAttendee = new MSupabaseEvent
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = attendeeId,
                        Title = eventData.Title,
                        Description = eventData.Description,
                        StartTime = eventData.StartTime,
                        EndTime = eventData.EndTime,
                        Priority = eventData.Priority,
                        Color = eventData.Color,
                        Attendees = eventData.Attendees ?? new List<object>()
                    };

                    await SupabaseConnection.SupabaseClient.From<MSupabaseEvent>().Insert(eventForAttendee);
                }
            }

            Logger.Info(TAG_USER, "Event added successfully.");
            return true;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Failed to add event: {error.Message}"); return false; }
    }

    public async Task<bool> DeleteCalendarEvent(string accessToken, string eventId)
    {
        Logger.Debug(TAG_USER, $"Deleting calendar event: {eventId}");
        try
        {
            var userResponse = await SupabaseConnection!.SupabaseClient.Auth.GetUser(accessToken);
            if (userResponse == null || userResponse.Id == null) { Logger.Warn(TAG_USER, "Invalid Access Token."); return false; }

            await SupabaseConnection.SupabaseClient.From<MSupabaseEvent>().Where(Event => Event.Id == eventId && Event.UserId == userResponse.Id).Delete();

            Logger.Info(TAG_USER, "Event deleted successfully.");
            return true;
        }
        catch (Exception error) { Logger.Error(TAG_USER, $"Failed to delete event: {error.Message}"); return false; }
    }
}