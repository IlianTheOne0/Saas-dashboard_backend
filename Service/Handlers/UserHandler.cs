namespace Service.Handlers;

using Microsoft.VisualBasic;
using Service.Interfaces;
using Service.Models;
using System.Linq;
using System.Text.Json;
using Utils;

public class UserFetchProfileHandler : AHandler, IHandler
{
    private const string TAG = "USER-PROFILE";

    private ISupabaseRepositoryUser _repository;

    public UserFetchProfileHandler(ISupabaseRepositoryUser repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");

        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }
            
            UserSessionDto data = _execute<UserSessionDto>(json);
            if (string.IsNullOrEmpty(data.AccessToken)) { Logger.Warn(TAG, "Session/Access Token is missing."); return new MResponse { status = "Error", message = "Session token required" }; }
            Logger.Debug(TAG, "Fetching profile from repository...");
            PersonalDataResultDto? result = await _repository.FetchProfile(data.AccessToken, null);

            if (result != null)
            {
                Logger.Info(TAG, "Success! Profile found.");

                return new MResponse { status = "Success", message = "User profile fetched successfully", data = JsonSerializer.SerializeToElement(result) };
            }
            else { Logger.Warn(TAG, "Profile not found or token invalid."); return new MResponse { status = "Error", message = "Profile not found or session expired" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during profile fetch", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}

public class UserFetchNonFavContactsHandler : AHandler, IHandler
{
    private const string TAG = "USERS-NON_FAN-CONTACTS";

    private ISupabaseRepositoryUser _repository;

    public UserFetchNonFavContactsHandler(ISupabaseRepositoryUser repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");

        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }

            UserSessionDto data = _execute<UserSessionDto>(json);
            if (string.IsNullOrEmpty(data.AccessToken)) { Logger.Warn(TAG, "Session/Access Token is missing."); return new MResponse { status = "Error", message = "Session token required" }; }

            Logger.Debug(TAG, "Fetching contacts from repository...");
            MSupabaseGroups? result = await _repository.FetchNonFavContacts(data.AccessToken);

            List<PersonalDataResultDto>? nonFanContacts = new List<PersonalDataResultDto>();
            if (result != null && result.ProfileIds != null)
            {
                foreach (var contactId in result.ProfileIds)
                {
                    Logger.Debug(TAG, $"Fetching non fav contact profile for ID: {contactId}");
                    PersonalDataResultDto? contactProfile = await _repository.FetchProfile(data.AccessToken, contactId);
                    if (contactProfile != null) { nonFanContacts.Add(contactProfile); }
                    else { Logger.Warn(TAG, $"Non fav contact profile not found for ID: {contactId}"); }
                }
            }

            if (nonFanContacts != null)
            {
                Logger.Info(TAG, "Success! Contacts found.");

                return new MResponse { status = "Success", message = "User profile fetched successfully", data = JsonSerializer.SerializeToElement(nonFanContacts) };
            }
            else { Logger.Warn(TAG, "Contacts not found or token invalid."); return new MResponse { status = "Error", message = "Contacts not found or session expired" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during non fav contacts fetch", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}

public class UserFetchFavContactsHandler : AHandler, IHandler
{
    private const string TAG = "USERS-FAV-CONTACTS";

    private ISupabaseRepositoryUser _repository;

    public UserFetchFavContactsHandler(ISupabaseRepositoryUser repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");

        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }

            UserSessionDto data = _execute<UserSessionDto>(json);
            if (string.IsNullOrEmpty(data.AccessToken)) { Logger.Warn(TAG, "Session/Access Token is missing."); return new MResponse { status = "Error", message = "Session token required" }; }

            Logger.Debug(TAG, "Fetching contacts from repository...");
            MSupabaseContacts? result = await _repository.FetchFavContacts(data.AccessToken);
            
            List<PersonalDataResultDto>? favContacts = new List<PersonalDataResultDto>();
            if (result != null && result.Contacts != null)
            {
                foreach (var contactId in result.Contacts)
                {
                    Logger.Debug(TAG, $"Fetching fav contact profile for ID: {contactId}");
                    var contactProfile = await _repository.FetchProfile(data.AccessToken, contactId);
                    if (contactProfile != null) { favContacts.Add(contactProfile); }
                    else { Logger.Warn(TAG, $"Fav contact profile not found for ID: {contactId}"); }
                }
            }

            if (favContacts != null)
            {
                Logger.Info(TAG, "Success! Contacts found.");

                return new MResponse { status = "Success", message = "User profile fetched successfully", data = JsonSerializer.SerializeToElement(favContacts) };
            }
            else { Logger.Warn(TAG, "Contacts not found or token invalid."); return new MResponse { status = "Error", message = "Contacts not found or session expired" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during faf contacts fetch", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}

public class UserStarContactHandler : AHandler, IHandler
{
    private const string TAG = "USER-STAR_CONTACTS";

    private ISupabaseRepositoryUser _repository;

    public UserStarContactHandler(ISupabaseRepositoryUser repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");

        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }

            UserSessionDto data = _execute<UserSessionDto>(json);
            if (string.IsNullOrEmpty(data.AccessToken)) { Logger.Warn(TAG, "Session/Access Token is missing."); return new MResponse { status = "Error", message = "Session token required" }; }

            ContactToStarDto contactToStar;
            try { contactToStar = data.data.Deserialize<ContactToStarDto>(); if (contactToStar == null) { throw new Exception("Null DTO"); } }
            catch { Logger.Warn(TAG, "Could not parse ContactToStarDto from session data."); return new MResponse { status = "Error", message = "Invalid data format for contact ID" }; }

            if (string.IsNullOrEmpty(contactToStar.ContactId)) { Logger.Warn(TAG, "Contact Id to star/unstar is missing."); return new MResponse { status = "Error", message = "Contact Id is missing." }; }

            Logger.Debug(TAG, $"Verifying existence of contact: {contactToStar.ContactId}");
            var targetProfile = await _repository.FetchProfile(data.AccessToken, contactToStar.ContactId);
            if (targetProfile == null) { Logger.Warn(TAG, "The user you are trying to add does not exist."); return new MResponse { status = "Error", message = "User does not exist" }; }

            Logger.Debug(TAG, "Fetching current favourite contacts...");
            MSupabaseContacts? result = await _repository.FetchFavContacts(data.AccessToken);

            if (result == null) { Logger.Warn(TAG, "Could not retrieve contacts table."); return new MResponse { status = "Error", message = "Internal database error" }; }

            if (result.Contacts == null) { result.Contacts = new List<string>(); }

            string actionTaken = "";
            if (result.Contacts.Contains(contactToStar.ContactId))
            {
                Logger.Info(TAG, $"Removing {contactToStar.ContactId} from favourites.");
                result.Contacts.Remove(contactToStar.ContactId);
                actionTaken = "removed";
            }
            else
            {
                Logger.Info(TAG, $"Adding {contactToStar.ContactId} to favourites.");
                result.Contacts.Add(contactToStar.ContactId);
                actionTaken = "added";
            }

            bool saveSuccess = await _repository.UpdateFavContacts(data.AccessToken, result.Contacts);

            if (saveSuccess) { Logger.Info(TAG, $"Success! Contact {actionTaken}."); return new MResponse { status = "Success", message = $"Contact successfully {actionTaken}" }; }
            else { Logger.Error(TAG, "Failed to save changes to database."); return new MResponse { status = "Error", message = "Database save failed" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during star contact execution", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}