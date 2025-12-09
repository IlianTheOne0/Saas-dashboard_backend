namespace Service.Models;

using System.Text.Json;

public class UserSessionDto
{
    public string AccessToken { get; set; } = string.Empty;
    public JsonElement data { get; set; }
}