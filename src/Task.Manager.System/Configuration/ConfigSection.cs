using Task.Manager.Cli.Utils;

namespace Task.Manager.System.Configuration;

public sealed class ConfigSection
{
    private string _name = string.Empty;
    private readonly Dictionary<string, string> _keys;

    internal ConfigSection() => _keys = new Dictionary<string, string>();

    public ConfigSection(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _keys = new Dictionary<string, string>();
    }
    
    public ConfigSection Add(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        _keys[key] = value;

        return this;
    }

    public ConfigSection AddIfMissing(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        if (false == _keys.ContainsKey(key)) {
            Add(key, value);
        }

        return this;
    }

    public bool Contains(string key) => _keys.ContainsKey(key);

    public string GetString(string key) => GetString(key, string.Empty);

    public string GetString(string key, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(defaultValue);
        
        if (false == Contains(key)) {
            return defaultValue;
        }

        return _keys[key];
    }

    public ConsoleColor GetColour(string key) => GetColour(key, ConsoleColor.Black);

    public ConsoleColor GetColour(string key, ConsoleColor defaultValue)
    {
        string colourName = GetString(key, string.Empty);

        if (string.IsNullOrEmpty(colourName)) {
            return defaultValue;
        }
        
        return ConsoleColorUtils.FromName(colourName, defaultValue);   
    }

    public int GetInt(string key) => GetInt(key, 0);

    public int GetInt(string key, int defaultValue) => TryParse(key, int.Parse, 0);

    public bool GetBool(string key) => GetBool(key, false);

    public bool GetBool(string key, bool defaultValue) => TryParse(key, bool.Parse, false);

    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(Name));
    }

    private T TryParse<T>(string key, Func<string, T> parseMethod, T defaultValue)
    {
        string strVal = GetString(key, string.Empty);

        if (string.IsNullOrWhiteSpace(strVal)) {
            return defaultValue;
        }

        try {
            return parseMethod(strVal);
        }
        catch {
            return defaultValue;
        }
    }
}
