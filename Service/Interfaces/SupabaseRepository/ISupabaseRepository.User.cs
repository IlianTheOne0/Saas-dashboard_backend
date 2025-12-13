namespace Service.Interfaces;

using Service.Models;

public interface ISupabaseRepositoryUser
{
    Task<PersonalDataResultDto?> FetchProfile(string accessToken, string? contactId);

    Task<MSupabaseGroups?> FetchNonFavContacts(string accessToken);
    Task<MSupabaseContacts?> FetchFavContacts(string accessToken);

    Task<bool> UpdateFavContacts(string accessToken, List<string> contacts);
    Task<bool> UpdateProfile(string accessToken, UpdateProfileDto data);

    Task<string?> UpdateAvatar(string accessToken, string base64Image, string fileName);
    Task<bool> DeleteAvatar(string accessToken);
}