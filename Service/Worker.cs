namespace Service.Worker;

using Base.Entities;
using Base.Models;
using Utils;

using System.Text.Json;

public partial class Worker : Base
{
    private const string TAG = "SERVICE-WORKER";

    private readonly Dictionary<string, Func<JsonElement, string, Task<string>>>? _handlers = null;

    public Worker()
    {
        Logger.Debug(TAG, "Registering Handlers...");
        
        _handlers = new Dictionary<string, Func<JsonElement, string, Task<string>>>
        {
            { "register", (data, cid) => _ForwardToDatabase("register", cid, data) },
            { "login", (data, cid) => _ForwardToDatabase("login", cid, data) },
            { "recovery_password", (data, cid) => _ForwardToDatabase("recovery_password", cid, data) },
            { "new_password", (data, cid) => _ForwardToDatabase("new_password", cid, data) },

            { "register-answer", (data, cid) => _ForwardToFrontend("register-answer", cid, data) },
            { "login-answer", (data, cid) => _ForwardToFrontend("login-answer", cid, data) },
            { "recovery_password-answer", (data, cid) => _ForwardToFrontend("recovery_password-answer", cid, data) },
            { "new_password-answer", (data, cid) => _ForwardToFrontend("new_password-answer", cid, data) },
        };

        Logger.Info(TAG, $"Handlers registered: {_handlers.Count}");
    }
}

public partial class Worker
{
    private string CreateErrorResponse(string message)
    {
        Logger.Warn(TAG, $"Generating Error Response: {message}");
        var wrapper = new MUnit() { Event = "error", Data = JsonSerializer.SerializeToElement(message) };
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
            return String.Empty;
        }
        catch (JsonException error) { Logger.Error(TAG, "JSON Parsing Error", error); return CreateErrorResponse("Invaild JSON format"); }
        catch (Exception error) { Logger.Error(TAG, "Internal Processing Error", error); return CreateErrorResponse("Internal server error"); }
    }
}

public partial class Worker
{
    private async Task<string> _ForwardToDatabase(string eventName, string cid, JsonElement data)
    {
        Logger.Info(TAG, $"[ROUTING] Forwarding '{eventName}' to Database...");

        var wrapper = new MUnit() { Event = eventName, CorrelationId = cid, Data = data };
        string payload = JsonSerializer.Serialize(wrapper);

        await produceMessage("database-topic", payload);

        return String.Empty;
    }

    private async Task<string> _ForwardToFrontend(string eventName, string cid, JsonElement data)
    {
        Logger.Info(TAG, $"[ROUTING] Forwarding '{eventName}' to Frontend...");

        var wrapper = new MUnit() { Event = eventName, CorrelationId = cid, Data = data };
        string payload = JsonSerializer.Serialize(wrapper);

        await produceMessage("auth-topic-answers", payload);

        return String.Empty;
    }
}