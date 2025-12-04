namespace Service.Interfaces;

using Service.Models;

public interface ISupabaseRepositoryUser
{
    Task<PersonalDataResultDto?> FetchProfile(string accessToken);
}