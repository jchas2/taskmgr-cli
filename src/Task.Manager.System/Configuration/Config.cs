using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System.Configuration;

public class Config
{
    private List<ConfigSection> _sections;

    public Config()
    {
        _sections = new List<ConfigSection>();
    }
    
    public static Config FromFile(IFileSystem fileSys, string path)
    {
        ArgumentNullException.ThrowIfNull(fileSys);
        ArgumentNullException.ThrowIfNull(path);

        if (false == fileSys.Exists(path))
        {
            throw new Exception();
        }

        string buf = fileSys.ReadAllText(path);
        return FromStringInternal(buf);
    }

    // Use a ConfigLoadException, ConfigParseException, derive from ConfigException for better error handling at the higher levels.

    public static Config FromString(in string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        return FromStringInternal(str);
    }

    private static Config FromStringInternal(string buf)
    {
        var config = new Config();
        return config;
    }

    public ConfigSection GetSection(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (false == ContainsSection(name)) {
            throw new InvalidOperationException();
        }

        return _sections.Single(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }

    public IList<ConfigSection> GetSections() => _sections;

    public bool ContainsSection(string name) =>
        _sections.Any(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
}
