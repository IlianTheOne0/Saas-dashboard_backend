namespace Base.Services;

using Base.Services.SSecurity;

using Base.Models;

using System.Text.Json;

public static class SecurityService
{
    private static string? _key;
    private static string? _iv;

    public static async Task LoadKeys()
    {
        try
        {
            const string keyPath = "./.config/encryption_keys.txt";
            if (!File.Exists(keyPath)) { throw new FileNotFoundException("Encryption keys file not found"); }

            string jsonString = await File.ReadAllTextAsync(keyPath);
            MConfig? config = JsonSerializer.Deserialize<MConfig>(jsonString);
            if (config == null || string.IsNullOrEmpty(config.Key) || string.IsNullOrEmpty(config.IV)) { throw new InvalidDataException("Invalid encryption keys data"); }

            _key = config.Key;
            _iv = config.Iv;

            Console.WriteLine("Encryption keys loaded successfully");
        }
        catch (Exception error) { Console.WriteLine($"Error loading encryption keys: {error.Message}"); throw; }
    }

    public static string Encrpyt(string plainText) => Encryptor.Execute(_key!, _iv!, plainText);
    public static string Decrypt(string cipherText) => Decryptor.Execute(_key!, _iv!, cipherText);
}