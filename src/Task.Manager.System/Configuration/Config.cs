using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System.Configuration;

public class Config
{
    private IList<ConfigSection> _sections;

    public Config()
    {
        _sections = new List<ConfigSection>();
    }

    public bool ContainsSection(string name) =>
        _sections.Any(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

    public static Config? FromFile(IFileSystem fileSys, string path)
    {
        ArgumentNullException.ThrowIfNull(fileSys);
        ArgumentNullException.ThrowIfNull(path);

        var parser = new ConfigParser(fileSys, path);
        return ParseConfig(parser);
    }

    public static Config? FromString(string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        var parser = new ConfigParser(str);
        return ParseConfig(parser);
    }
    
    public ConfigSection GetSection(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (false == ContainsSection(name)) {
            throw new InvalidOperationException();
        }

        return _sections.Single(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public IList<ConfigSection> Sections
    {
        get => _sections;
        private set => _sections = value;
    }

    private static void TryLoadConfig(Action action)
    {
        try {
            action.Invoke();
        }
        catch (Exception e) {
            throw new ConfigLoadException(e.Message, e);
        }
    }
    
    private static Config ParseConfig(ConfigParser parser)
    {
        var config = new Config();
        parser.Parse(); 
        config.Sections = parser.Sections;
        return config;
    }
}
