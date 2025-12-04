namespace Service.Models;

using System.Text.Json;

public class MResponse
{
    public string status { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public JsonElement? data { get; set; }

    public MResponse() { status = "Error"; message = "Unknown Backend Error"; data = null; }
}