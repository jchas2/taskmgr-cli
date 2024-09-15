using Task.Manager.Internal.Abstractions;
using System.Linq;
using System.Reflection;

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
        
        return TryParseConfig(parser, out Config config) 
            ? config 
            : null;
    }

    // Use a ConfigLoadException, ConfigParseException, derive from ConfigException for better error handling at the higher levels.

    public static Config? FromString(string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        var parser = new ConfigParser(str);
        
        return TryParseConfig(parser, out Config config) 
            ? config 
            : null;
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
    
    public IList<ConfigSection> Sections
    {
        get => _sections;
        private set => _sections = value;
    }
    
    private static bool TryParseConfig(ConfigParser parser, out Config config)
    {
        config = new Config();
        bool result = parser.Parse();
        
        if (false == result) {
            return false;
        }

        config.Sections = parser.Sections;
        return true;
    }
}
