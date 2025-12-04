namespace Service.Handlers;

using Service.Interfaces;
using Service.Models;
using Base.Models;
using Utils;

using System.Text.Json;

public class PersonalDataHandler : AHandler, IHandler, IPersonalDataHandler
{
    private const string TAG = "PERSONAL_DATA-HANDLER";

    private readonly Func<string, string, Task> _produceMessage;

    private JsonElement? _profile = null;

    public PersonalDataHandler(Func<string, string, Task> produceMessage) => _produceMessage = produceMessage;

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");

        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }

            Logger.Debug(TAG, $"Forwarding DTO to the database for accessToken");

            var fetchProfile_payload = new MUnit
            {
                Event = "fetch_profile",
                Data = json
            };
            string fetchProfile_payloadString = JsonSerializer.Serialize(fetchProfile_payload);

            Logger.Info(TAG, "Forwarding request to Database Service...");
            await _produceMessage("database", fetchProfile_payloadString);

            return new MResponse { status = "Success", message = "Successfully sent requests to the database to obtain the user's personal data" };
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during fetching of the user's data", error); return new MResponse { status = "Error", message = error.Message }; }
    }

    public MResponse SetProfile(JsonElement data)
    {
        Logger.Debug(TAG, "Setting profile data...");
        _profile = data;

        MResponse result = CompleteTheWork();

        Logger.Debug(TAG, "Sucess! Returning the result of completed work");
        return result;
    }

    public MResponse CompleteTheWork()
    {
        Logger.Debug(TAG, "Completing the work with profile data set...");
        
        if (_profile == null) { Logger.Error(TAG, "Cannot complete work: Profile data is missing"); throw new InvalidOperationException("Profile data is missing"); }

        JsonElement profileData = _profile.Value.TryGetProperty("data", out var pData) ? pData : _profile.Value;

        MResponse result = new MResponse
        {
            status = "Success",
            message = "Successfully retrieved personal data",
            data = JsonSerializer.SerializeToElement
            (
                new PersonalDataResultDto
                {
                    Id = GetValue(profileData, "Id") ?? String.Empty,
                    Email = GetValue(profileData, "Email") ?? string.Empty,
                    Name = GetValue(profileData, "Name") ?? string.Empty,
                    AvatarUrl = GetValue(profileData, "AvatarUrl") ?? string.Empty
                }
            )
        };

        _profile = null;

        return result;
    }

    private string? GetValue(JsonElement element, string key)
    {
        if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined) return null;
        if (element.TryGetProperty(key, out var value)) { return value.ValueKind == JsonValueKind.Null ? null : value.GetString(); }

        string camelKey = char.ToLower(key[0]) + key.Substring(1);
        if (element.TryGetProperty(camelKey, out value)) { return value.ValueKind == JsonValueKind.Null ? null : value.GetString(); }

        return null;
    }
}