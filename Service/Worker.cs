namespace Service.Worker;

using Base.Entities;
using Base.Models;
using Service.Handlers;
using Service.Interfaces;
using Utils;

using System.Text.Json;

public class Worker : Base
{
    private const string TAG = "SERVICE-WORKER";

    private readonly Dictionary<string, Func<JsonElement, Task<string>>>? _handlers = null;

    private string CreateMUnitResponse(string eventName, object message)
    {
        Logger.Warn(TAG, $"Generating Error Response: {message}");
        var wrapper = new MUnit() { Event = eventName, Data = JsonSerializer.SerializeToElement(message) };
        return JsonSerializer.Serialize(wrapper);
    }

    protected override async Task<string> ProcessMessage(string message)
    {
        try
        {
            var unit = JsonSerializer.Deserialize<MUnit>(message);
            if (unit == null || string.IsNullOrWhiteSpace(unit.Event)) { Logger.Warn(TAG, "Invalid message format received."); throw new Exception("Invalid message format"); }

            Logger.Info(TAG, $"Routing to handler for event: '{unit.Event}'");

            if (_handlers == null) { throw new Exception("No handlers are registered"); }

            if (_handlers.TryGetValue(unit.Event, out var handler))
            {
                Logger.Debug(TAG, "Handler found. Executing...");
                var result = await handler(unit.Data);
                Logger.Debug(TAG, "Handler execution complete.");
                return result;
            }

            Logger.Warn(TAG, $"No handler registered for: {unit.Event}");
            return string.Empty;
        }
        catch (JsonException error) { Logger.Error(TAG, "JSON Parsing Error", error); return CreateMUnitResponse("error", "Invaild JSON format"); }
        catch (Exception error) { Logger.Error(TAG, "Internal Processing Error", error); return CreateMUnitResponse("error", "Internal server error"); }
    }

    public Worker(ISupabaseRepositoryAuth RAuth)
    {
        Logger.Debug(TAG, "Registering Handlers...");

        AuthRegisterHandler AuthRegisterHandler = new AuthRegisterHandler(RAuth);
        AuthLoginHandler AuthLoginHandler = new AuthLoginHandler(RAuth);
        AuthRecoveryPasswordHandler AuthRecoveryPasswordHandler = new AuthRecoveryPasswordHandler(RAuth);
        AuthNewPasswordHandler AuthNewPasswordHandler = new AuthNewPasswordHandler(RAuth);

        _handlers = new Dictionary<string, Func<JsonElement, Task<string>>>
        {
            {
                "register",
                async (data) =>
                {
                    Logger.Debug(TAG, "Executing 'register'...");
                    var response = await AuthRegisterHandler.Execute(data);
                    return CreateMUnitResponse("register-answer", response);
                }
            },
            {
                "login",
                async (data) =>
                {
                    Logger.Debug(TAG, "Executing 'login'...");
                    var response = await AuthLoginHandler.Execute(data);
                    return CreateMUnitResponse("login-answer", response);
                }
            },
            {
                "recovery_password",
                async (data) =>
                {
                    Logger.Debug(TAG, "Executing 'recovery_password'...");
                    var response = await AuthRecoveryPasswordHandler.Execute(data);
                    return CreateMUnitResponse("recovery_password-answer", response);
                }
            },
            {
                "new_password",
                async (data) =>
                {
                    Logger.Debug(TAG, "Executing 'new_password'...");
                    var response = await AuthNewPasswordHandler.Execute(data);
                    return CreateMUnitResponse("new_password-answer", response);
                }
            }
        };

        Logger.Info(TAG, $"Handlers registered: {_handlers.Count}");
    }
}