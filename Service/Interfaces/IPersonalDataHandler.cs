namespace Service.Interfaces;

using Service.Models;

using System.Text.Json;

public interface IPersonalDataHandler
{
    MResponse SetProfile(JsonElement data);
    
    MResponse CompleteTheWork();
}