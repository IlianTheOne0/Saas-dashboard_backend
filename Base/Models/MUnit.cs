namespace Base.Models;

using System.Text.Json;

public class MUnit
{
    public string Event { get; set; } = string.Empty;
    public JsonElement Data { get; set; }
}