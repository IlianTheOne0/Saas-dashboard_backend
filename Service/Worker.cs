namespace Service.Worker;

using Service.Handlers;
using Base.Entities;
using Base.Models;
using Utils;

using System.Text.Json;
using Service.Models;

public class Worker : Base
{
    private const string TAG = "SERVICE-WORKER";

    private readonly Dictionary<string, Func<JsonElement, string, Task<string>>>? _handlers = null;

    private string CreateMUnitResponse(string eventName, string cid, object message)
    {
        Logger.Warn(TAG, $"Generating Response: {message}");
        var wrapper = new MUnit() { Event = eventName, CorrelationId = cid, Data = JsonSerializer.SerializeToElement(message) };
        return JsonSerializer.Serialize(wrapper);
    }

    protected override async Task<string> ProcessMessage(string message)
    {
        string currentCorrelationId = "";

        try
        {
            var unit = JsonSerializer.Deserialize<MUnit>(message);
            if (unit == null || string.IsNullOrWhiteSpace(unit.Event)) { Logger.Warn(TAG, "Invalid message format received."); throw new Exception("Invalid message format"); }

            currentCorrelationId = unit.CorrelationId;

            Logger.Info(TAG, $"Routing '{unit.Event}' with such correlation id: '{currentCorrelationId}'");

            if (_handlers == null) { throw new Exception("No handlers are registered"); }

            if (_handlers.TryGetValue(unit.Event, out var handler))
            {
                Logger.Debug(TAG, "Handler found. Executing...");
                var result = await handler(unit.Data, currentCorrelationId);
                Logger.Debug(TAG, "Handler execution complete.");
                return result;
            }

            Logger.Warn(TAG, $"No handler registered for: {unit.Event}");
            return string.Empty;
        }
        catch (JsonException error) { Logger.Error(TAG, "JSON Parsing Error", error); return CreateMUnitResponse("error", currentCorrelationId, "Invaild JSON format"); }
        catch (Exception error) { Logger.Error(TAG, "Internal Processing Error", error); return CreateMUnitResponse("error", currentCorrelationId, "Internal server error"); }
    }

    public Worker()
    {
        Logger.Debug(TAG, "Registering Handlers...");

        PersonalDataHandler personalDataHandler = new PersonalDataHandler(produceMessage);
        ContactsHandler contactsHandler = new ContactsHandler(produceMessage);

        _handlers = new Dictionary<string, Func<JsonElement, string, Task<string>>>
        {
            {
                "get_personal_data",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_personal_data'...");
                    
                    await personalDataHandler.Execute(data, cid);
                    return string.Empty;
                }
            },
            {
                "fetch_profile-answer",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Received 'fetch_profile-answer'");
                    MResponse response = personalDataHandler.SetProfile(data);
                    return CreateMUnitResponse("get_personal_data-answer", cid, response);
                }
            },

            {
                "get_all_contacts",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_all_contacts'...");
                    await contactsHandler.Execute(data, cid, true, true);
                    return string.Empty;
                }
            },
            {
                "fetch_non_favourite_contacts-answer",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Received 'fetch_non_favourite_contacts-answer'");
                    MResponse? response = contactsHandler.SetNonFavouriteContacts(data, cid);

                    if (response != null) { return CreateMUnitResponse("get_all_contacts-answer", cid, response); }
                    return string.Empty;
                }
            },
            {
                "fetch_favourite_contacts-answer",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Received 'fetch_favourite_contacts-answer'");
                    MResponse? response = contactsHandler.SetFavouriteContacts(data, cid);

                    if (response != null) { return CreateMUnitResponse("get_all_contacts-answer", cid, response); } return string.Empty;
                }
            },

            {
                "star_contact",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'star_contact'...");
                    await produceMessage("database", JsonSerializer.Serialize(new MUnit { Event = "star_contact", CorrelationId = cid, Data = data }));
                    return string.Empty;
                }
            },

            {
                "update_profile",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Routing 'update_profile' to database...");
                    await produceMessage("database", JsonSerializer.Serialize(new MUnit { Event = "update_profile", CorrelationId = cid, Data = data }));
                    return string.Empty;
                }
            },
            {
                "update_profile-answer",
                async (data, cid) => CreateMUnitResponse("update_profile-answer", cid, data.Deserialize<MResponse>())
            },
            {
                "update_email",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Routing 'update_email' to database...");
                    await produceMessage("database", JsonSerializer.Serialize(new MUnit { Event = "update_email", CorrelationId = cid, Data = data }));
                    return string.Empty;
                }
            },
            {
                "update_email-answer",
                async (data, cid) => CreateMUnitResponse("update_email-answer", cid, data.Deserialize<MResponse>())
            },
            {
                "change_password",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Routing 'change_password' to database...");
                    await produceMessage("database", JsonSerializer.Serialize(new MUnit { Event = "change_password", CorrelationId = cid, Data = data }));
                    return string.Empty;
                }
            },
            {
                "change_password-answer",
                async (data, cid) => CreateMUnitResponse("change_password-answer", cid, data.Deserialize<MResponse>())
            },

            {
                "upload_avatar",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Routing 'upload_avatar' to database...");
                    await produceMessage("database", JsonSerializer.Serialize(new MUnit { Event = "upload_avatar", CorrelationId = cid, Data = data }));
                    return string.Empty;
                }
            },
            {
                "upload_avatar-answer",
                async (data, cid) => CreateMUnitResponse("upload_avatar-answer", cid, data.Deserialize<MResponse>())
            },
            {
                "delete_avatar",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Routing 'delete_avatar' to database...");
                    await produceMessage("database", JsonSerializer.Serialize(new MUnit { Event = "delete_avatar", CorrelationId = cid, Data = data }));
                    return string.Empty;
                }
            },
            {
                "delete_avatar-answer",
                async (data, cid) => CreateMUnitResponse("delete_avatar-answer", cid, data.Deserialize<MResponse>())
            },
        };

        Logger.Info(TAG, $"Handlers registered: {_handlers.Count}");
    }
}