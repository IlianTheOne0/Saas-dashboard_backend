namespace Service.Handlers;

using Service.Interfaces;
using Service.Models;
using Base.Models;
using Utils;

using System.Text.Json;
using System.Collections.Concurrent;

public class ContactsHandler : AHandler, IContactsHandler, IHandler
{
    private const string TAG = "CONTACTS-HANDLER";

    private readonly Func<string, string, Task> _produceMessage;

    private readonly ConcurrentDictionary<string, ContactRequestState> _pendingRequests = new();

    public ContactsHandler(Func<string, string, Task> produceMessage) => _produceMessage = produceMessage;

    public async Task<MResponse> Execute(JsonElement json, string cid) => await Execute(json, cid, true, true);

    public async Task<MResponse> Execute(JsonElement json, string cid, bool isNonFavouriteNeeded, bool isFavouriteNeeded)
    {
        Logger.Debug(TAG, "Processing request...");

        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }

            Logger.Debug(TAG, $"Forwarding DTO to the database for accessToken");

            var requestState = new ContactRequestState();

            if (!isNonFavouriteNeeded) requestState.IsNonFavReceived = true;
            if (!isFavouriteNeeded) requestState.IsFavReceived = true;

            _pendingRequests.TryAdd(cid, requestState);

            if (isNonFavouriteNeeded)
            {
                var fetchNonFavPayload = new MUnit { Event = "fetch_non_favourite_contacts", CorrelationId = cid, Data = json };
                string payloadString = JsonSerializer.Serialize(fetchNonFavPayload);
                await _produceMessage("database", payloadString);
                Logger.Info(TAG, $"[CID: {cid}] Sent 'fetch_non_favourite_contacts' to Database.");
            }

            if (isFavouriteNeeded)
            {
                var fetchFavPayload = new MUnit { Event = "fetch_favourite_contacts", CorrelationId = cid, Data = json };
                string payloadString = JsonSerializer.Serialize(fetchFavPayload);
                await _produceMessage("database", payloadString);
                Logger.Info(TAG, $"[CID: {cid}] Sent 'fetch_favourite_contacts' to Database.");
            }

            return new MResponse { status = "Pending", message = "Requests sent to database" };
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during fetching of the user's data", error); _pendingRequests.TryRemove(cid, out _); return new MResponse { status = "Error", message = error.Message }; }
    }

    public MResponse? SetNonFavouriteContacts(JsonElement data, string cid)
    {
        Logger.Debug(TAG, $"[CID: {cid}] Received Non-Favourite contacts.");

        if (_pendingRequests.TryGetValue(cid, out var state))
        {
            state.NonFavouriteContacts = _TryGetContactsData(data, "NonFav");
            state.IsNonFavReceived = true;
            return CheckAndComplete(cid, state);
        }

        Logger.Warn(TAG, $"[CID: {cid}] Received data for unknown or expired request.");
        return null;
    }

    public MResponse? SetFavouriteContacts(JsonElement data, string cid)
    {
        Logger.Debug(TAG, $"[CID: {cid}] Received Favourite contacts.");

        if (_pendingRequests.TryGetValue(cid, out var state))
        {
            state.FavouriteContacts = _TryGetContactsData(data, "Fav");
            state.IsFavReceived = true;
            return CheckAndComplete(cid, state);
        }

        Logger.Warn(TAG, $"[CID: {cid}] Received data for unknown or expired request.");
        return null;
    }

    private JsonElement? _TryGetContactsData(JsonElement data, string contactType)
    {
        try
        {
            var response = data.Deserialize<MResponse>();

            if (response != null && response.status == "Success" && response.data.HasValue){ return response.data.Value; }

            Logger.Warn(TAG, $"Database returned error or empty for {contactType}: {response?.message ?? "No response"}");
            return null;
        }
        catch (Exception error) { Logger.Error(TAG, $"Failed to parse {contactType} contacts data", error); return null; }
    }

    private MResponse? CheckAndComplete(string cid, ContactRequestState state)
    {
        if (state.IsComplete)
        {
            Logger.Info(TAG, $"[CID: {cid}] Data aggregation complete. Building response.");

            DtoContacts resultData = new DtoContacts
            {
                NonFavouriteContacts = state.NonFavouriteContacts,
                FavouriteContacts = state.FavouriteContacts
            };

            _pendingRequests.TryRemove(cid, out _);

            return new MResponse { status = "Success", message = "Successfully retrieved contacts data", data = JsonSerializer.SerializeToElement(resultData) };
        }

        Logger.Debug(TAG, $"[CID: {cid}] Still waiting for other data sources...");
        return null;
    }
}