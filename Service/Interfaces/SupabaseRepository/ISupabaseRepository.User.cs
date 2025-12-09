namespace Service.Interfaces;

using Service.Models;

public interface ISupabaseRepositoryUser
{
    Task<PersonalDataResultDto?> FetchProfile(string accessToken, string? contactId);

    Task<MSupabaseGroups?> FetchNonFavContacts(string accessToken);
    Task<MSupabaseContacts?> FetchFavContacts(string accessToken);

    Task<bool> UpdateFavContacts(string accessToken, List<string> contacts);
}