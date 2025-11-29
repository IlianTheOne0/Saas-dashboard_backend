namespace Service.Worker;

using Base.Entities;
using Base.Models;

using System.Text.Json;

public class Worker : Base
{
    private readonly Dictionary<string, Func<JsonElement, Task<string>>>? _handlers = null;

    //private async Task<string> _handleEvent(JsonElement data)
    //{
    //    var dto = data.Deserialize<Dto>();
    //    some work
    //    return JsonSerializer.Serialize(new { status = "success", token = "xyz-123" });
    //}

    private string CreateErrorResponse(string message) => JsonSerializer.Serialize(new { status = "error", message = message });

    protected override async Task<string> ProcessMessage(string message)
    {
        try
        {
            var unit = JsonSerializer.Deserialize<MUnit>(message);
            if (unit == null || string.IsNullOrWhiteSpace(unit.Event)) { throw new Exception("Invalid message format"); }

            Console.WriteLine($"Routing to handler for event: {unit.Event}");

            if (_handlers == null) { throw new Exception("No handlers are registered"); }
            if (_handlers.TryGetValue(unit.Event, out var handler)) { return await handler(unit.Data); }
            throw new Exception($"No handler found for event: {unit.Event}");
        }
        catch (JsonException) { return CreateErrorResponse("Invaild JSON format"); }
        catch (Exception) { return CreateErrorResponse("Internal server error"); }
    }

    public Worker()
    {
        _handlers = new Dictionary<string, Func<JsonElement, Task<string>>>
        {
            //{ "event", _handleEvent }
        };
    }
}