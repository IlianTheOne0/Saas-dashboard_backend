namespace Data.Entities;

using Data.Entities.Interfaces;
using Data.Entities.Abstracts;

public class SettingsReader : AReader, ISettingsReader
{
    public SettingsReader() : base("Settings/") { }

    public Task<string> GetFaqsData() => Task.Run(() => Read("faqs"));
}