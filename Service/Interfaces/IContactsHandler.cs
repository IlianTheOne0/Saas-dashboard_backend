namespace Service.Interfaces;

using Service.Models;

using System.Text.Json;

public interface IContactsHandler
{
    Task<MResponse> Execute(JsonElement json, string cid, bool isNonFavouriteNeeded, bool isFavouriteNeeded);

    MResponse? SetNonFavouriteContacts(JsonElement data, string cid);
    MResponse? SetFavouriteContacts(JsonElement data, string cid);
}