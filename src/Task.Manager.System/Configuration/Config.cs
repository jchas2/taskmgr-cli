using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System.Configuration;

public class Config
{
    private IList<ConfigSection> _configSections;

    public Config()
    {
        _configSections = new List<ConfigSection>();
    }

    public Config AddConfigSection(ConfigSection section)
    {
        if (ContainsSection(section.Name)) {
            throw new InvalidOperationException($"A section with name {section.Name} already exists.");
        }
        
        ConfigSections.Add(section);
        return this;
    }
    
    public bool ContainsSection(string name) =>
        _configSections.Any(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));

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
    
    public ConfigSection GetConfigSection(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (false == ContainsSection(name)) {
            throw new InvalidOperationException();
        }

        return _configSections.Single(s => s.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }
    
    public IList<ConfigSection> ConfigSections
    {
        get => _configSections;
        private set => _configSections = value;
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
        config.ConfigSections = parser.Sections;
        return config;
    }
}
