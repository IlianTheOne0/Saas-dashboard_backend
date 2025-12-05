namespace Service.Handlers;

using Service.Interfaces;
using Service.Models;
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
            PersonalDataResultDto? result = await _repository.FetchProfile(data.AccessToken);

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