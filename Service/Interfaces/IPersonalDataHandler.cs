namespace Service.Interfaces;

using Service.Models;

using System.Text.Json;

public interface IPersonalDataHandler
{
    public MResponse SetProfile(JsonElement data);
    
    public MResponse CompleteTheWork();
}