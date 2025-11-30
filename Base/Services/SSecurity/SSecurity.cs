namespace Base.Services;

using Base.Services.SSecurity;
using Base.Models;
using Utils;

using System.Text.Json;

public static class SecurityService
{
    private const string TAG = "SECURITY-SERVICE";

    private static string? _key;
    private static string? _iv;

    public static async Task LoadKeys()
    {
        try
        {
            Logger.Debug(TAG, "Loading encryption keys...");
            const string keyPath = "../.config/encryption_keys.json";
            if (!File.Exists(keyPath)) { throw new FileNotFoundException("Encryption keys file not found"); }

            // absolute path for debugging
            Console.WriteLine($"[DEBUG] Loading encryption keys from: {Path.GetFullPath(keyPath)}");
            string jsonString = await File.ReadAllTextAsync(keyPath);
            MConfig? config = JsonSerializer.Deserialize<MConfig>(jsonString);
            if (config == null || string.IsNullOrEmpty(config.Key) || string.IsNullOrEmpty(config.Iv)) { throw new InvalidDataException("Invalid encryption keys data"); }

            _key = config.Key;
            _iv = config.Iv;

            var keyBytes = Convert.FromHexString(_key);
            var ivBytes = Convert.FromHexString(_iv);

            if (ivBytes.Length != 16) { throw new ArgumentException($"IV must be 16 bytes (32 hex chars) for AES. Current size: {ivBytes.Length} bytes."); }
            if (keyBytes.Length != 16 && keyBytes.Length != 32) { throw new ArgumentException($"Key must be 16 or 32 bytes (32 or 64 hex chars). Current size: {keyBytes.Length} bytes."); }

            Logger.Info(TAG, "Encryption keys loaded successfully.");
        }
        catch (Exception error) { Logger.Fatal(TAG, "Failed to load encryption keys", error); throw; }
    }

    public static string Encrpyt(string plainText) => Encryptor.Execute(_key!, _iv!, plainText);
    public static string Decrypt(string cipherText) => Decryptor.Execute(_key!, _iv!, cipherText);
}