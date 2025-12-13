namespace Data.Entities.Interfaces;

public interface ISettingsReader
{
    Task<string> GetFaqsData();
}