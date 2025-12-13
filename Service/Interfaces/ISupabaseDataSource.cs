namespace Service.Interfaces;

using Supabase;

public interface ISupabaseDataSource
{
    Client SupabaseClient { get; }

    Task Initialize();

    Task<string?> UploadAvatar(byte[] fileData, string fileName);
    Task<bool> DeleteAvatar(string fileName);
}