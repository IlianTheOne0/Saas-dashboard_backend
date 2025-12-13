namespace Data.Entities.Abstracts;

public abstract class AReader
{
    private const string DEFAULT_PATH = "../Data/";
    private const string DEFAULT_EXTENSION = ".json";

    private string _path;

    protected AReader(string path) => _path = path;

    protected string Read(string fileName)
    {
        var fullPath = $"{DEFAULT_PATH}{_path}{fileName}{DEFAULT_EXTENSION}";
        return File.ReadAllText(fullPath);
    }
}