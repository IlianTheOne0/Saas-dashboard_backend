namespace Service.Interfaces;

using Supabase;

public interface ISupabaseDataSource
{
    Client SupabaseClient { get; }

    Task Initialize();
}