namespace Service.Repositories;

using System.Text.Json;

public abstract class ADatabaseRepository
{
    protected async Task<T> _initDatabase<T>(string keyPath)
    {
        if (keyPath == null) { throw new ArgumentNullException("File path cannot be null!"); }
        if (!File.Exists(keyPath)) { throw new FileNotFoundException("Database keys file not found"); }

        string jsonString = await File.ReadAllTextAsync(keyPath);
        T? config = JsonSerializer.Deserialize<T>(jsonString); 
        if (config == null) { throw new InvalidDataException("Invalid encryption keys data"); }

        return config;
    }
}