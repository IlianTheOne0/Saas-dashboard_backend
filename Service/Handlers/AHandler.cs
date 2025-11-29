namespace Service.Handlers;

using Service.Interfaces;
using System.Text.Json;

public abstract class AHandler
{
    protected T _execute<T>(JsonElement data)
    {
        T? dto = data.Deserialize<T>();
        if (dto == null) { throw new Exception("Failed to deserialize data"); }

        return dto;
    }
}