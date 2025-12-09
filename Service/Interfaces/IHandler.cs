namespace Service.Interfaces;

using Service.Models;

using System.Text.Json;

public interface IHandler
{
    Task<MResponse> Execute(JsonElement json, string cid);
}