using System.Text.Json;

namespace Service.Models;

public class MResponse
{
    public string status { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public JsonElement? data { get; set; }

    public MResponse() { status = "Error"; message = "Unknown Backend Error"; data = null; }
}