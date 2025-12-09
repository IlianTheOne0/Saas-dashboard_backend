using System.Text.Json;

namespace Service.Models;

public class DtoContacts
{
    public JsonElement? NonFavouriteContacts { get; set; } = null;
    public JsonElement? FavouriteContacts { get; set; } = null;
}

public class ContactRequestState
{
    public JsonElement? NonFavouriteContacts { get; set; }
    public JsonElement? FavouriteContacts { get; set; }

    public bool IsNonFavReceived { get; set; } = false;
    public bool IsFavReceived { get; set; } = false;

    public bool IsComplete => IsNonFavReceived && IsFavReceived;
}