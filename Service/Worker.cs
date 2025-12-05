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

        _handlers = new Dictionary<string, Func<JsonElement, string, Task<string>>>
        {
            {
                "get_personal_data",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_personal_data'...");
                    MResponse response = await personalDataHandler.Execute(data, cid);
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
            }
        };

        Logger.Info(TAG, $"Handlers registered: {_handlers.Count}");
    }
}