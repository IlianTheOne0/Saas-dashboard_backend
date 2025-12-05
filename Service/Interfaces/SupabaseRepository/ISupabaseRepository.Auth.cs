namespace Service.Interfaces;

using Supabase.Gotrue;

public interface ISupabaseRepositoryAuth
{
    Task<bool> RegisterUser(string email, string password, string username);
    Task<string?> LoginUser(string email, string password);
    Task<bool> RecoveryPassword(string email);
    Task<bool> SetNewPassword(string password, string accessToken, string refreshToken);
}