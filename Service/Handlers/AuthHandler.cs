namespace Service.Handlers;

using Service.Interfaces;
using Service.Models;
using Utils;

using Supabase.Gotrue;
using System.Text.Json;

public class AuthRegisterHandler : AHandler, IHandler
{
    private const string TAG = "AUTH-REGISTER";

    private ISupabaseRepositoryAuth _repository;

    public AuthRegisterHandler(ISupabaseRepositoryAuth repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");

        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }

            AuthDto data = _execute<AuthDto>(json);
            Logger.Debug(TAG, $"Parsed DTO for Email: {data.email}, Name: {data.name}");

            bool success = await _repository.RegisterUser(data.email, data.password, data.name);

            if (success) { Logger.Info(TAG, "Success!"); return new MResponse { status = "Success", message = "User registered successfully" }; }
            else { Logger.Warn(TAG, "Registration failed (unknown reason)."); return new MResponse { status = "Error", message = "Registration failed (unknown reason)" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during registration", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}

public class AuthLoginHandler : AHandler, IHandler
{
    private const string TAG = "AUTH-LOGIN";
    private ISupabaseRepositoryAuth _repository;

    public AuthLoginHandler(ISupabaseRepositoryAuth repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");
        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }
            AuthDto data = _execute<AuthDto>(json);
            Logger.Debug(TAG, $"Parsed DTO for Email: {data.email}");

            Session? session = await _repository.LoginUser(data.email, data.password);

            if (session != null)
            {
                Logger.Info(TAG, "Success!");

                return new MResponse { status = "Success", message = "User registered successfully", data = JsonSerializer.SerializeToElement(session) };
            }
            else { Logger.Warn(TAG, "Login failed."); return new MResponse { status = "Error", message = "Logining failed (unknown reason)" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during login", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}

public class AuthRecoveryPasswordHandler : AHandler, IHandler
{
    private const string TAG = "AUTH-RECOVERY";
    private ISupabaseRepositoryAuth _repository;

    public AuthRecoveryPasswordHandler(ISupabaseRepositoryAuth repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");
        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }
            AuthDto data = _execute<AuthDto>(json);
            Logger.Debug(TAG, $"Parsed DTO for Email: {data.email}");

            bool success = await _repository.RecoveryPassword(data.email);
            
            if (success) { Logger.Info(TAG, "Success!"); return new MResponse { status = "Success", message = "An email has been sent to the user to reset their password" }; }
            else { Logger.Warn(TAG, "Recovery failed."); return new MResponse { status = "Error", message = "Recovering of the password failed (unknown reason)" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during recovery", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}

public class AuthNewPasswordHandler : AHandler, IHandler
{
    private const string TAG = "AUTH-NEW_PASSWORD";
    private ISupabaseRepositoryAuth _repository;

    public AuthNewPasswordHandler(ISupabaseRepositoryAuth repository) { _repository = repository; }

    public async Task<MResponse> Execute(JsonElement json)
    {
        Logger.Debug(TAG, "Processing request...");
        try
        {
            if (json.ValueKind == JsonValueKind.Undefined || json.ValueKind == JsonValueKind.Null) { Logger.Warn(TAG, "No data provided in payload."); return new MResponse { status = "Error", message = "No data provided" }; }
            AuthNewPasswordDto data = _execute<AuthNewPasswordDto>(json);
            Logger.Debug(TAG, $"Parsed DTO for password, access_token and recovery_token!");

            bool success = await _repository.SetNewPassword(data.password, data.accessToken, data.refreshToken);
            
            if (success) { Logger.Info(TAG, "Success!"); return new MResponse { status = "Success", message = "User password successfully reset" }; }
            else { Logger.Warn(TAG, "Recovery failed."); return new MResponse { status = "Error", message = "Setting of the new password failed (unknown reason)" }; }
        }
        catch (Exception error) { Logger.Error(TAG, "Exception during recovery", error); return new MResponse { status = "Error", message = error.Message }; }
    }
}