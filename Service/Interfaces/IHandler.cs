namespace Service.Interfaces;

using Service.Models;

using System.Text.Json;

public interface IHandler
{
    public Task<MResponse> Execute(JsonElement data);
}