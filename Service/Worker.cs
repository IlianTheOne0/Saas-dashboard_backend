namespace Service.Worker;

using Base.Entities;
using Base.Models;
using Data.Entities.Interfaces;
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

    public Worker(IHomeReader homeReader, ISettingsReader settingsReader)
    {
        Logger.Debug(TAG, "Registering Handlers...");

        _handlers = new Dictionary<string, Func<JsonElement, string, Task<string>>>
        {
            {
                "get_home_dashboard",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_home_dashboard'...");
                    string response = await homeReader.GetDashboardData();
                    return CreateMUnitResponse("get_home_dashboard-answer", cid, response);
                }
            },
            {
                "get_home_realtime",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_home_realtime'...");
                    string response = await homeReader.GetRealtimeData();
                    return CreateMUnitResponse("get_home_realtime-answer", cid, response);
                }
            },
            {
                "get_home_audience",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_home_audience'...");
                    string response = await homeReader.GetAudienceData();
                    return CreateMUnitResponse("get_home_audience-answer", cid, response);
                }
            },
            {
                "get_home_traffic-source",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_home_traffic-source'...");
                    string response = await homeReader.GetTrafficSourceData();
                    return CreateMUnitResponse("get_home_traffic-source-answer", cid, response);
                }
            },

            {
                "get_settings_faqs",
                async (data, cid) =>
                {
                    Logger.Debug(TAG, "Executing 'get_settings_faqs'...");
                    string response = await settingsReader.GetFaqsData();
                    return CreateMUnitResponse("get_settings_faqs-answer", cid, response);
                }
            }
        };

        Logger.Info(TAG, $"Handlers registered: {_handlers.Count}");
    }
}