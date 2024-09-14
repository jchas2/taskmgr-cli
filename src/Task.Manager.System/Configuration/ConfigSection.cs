using System.Drawing;

namespace Task.Manager.System.Configuration;

public class ConfigSection
{
    private readonly string _name;
    private readonly Dictionary<string, string> _keys;

    public ConfigSection(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        _name = name;
        _keys = new Dictionary<string, string>();
    }

    public void Add(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        _keys[key] = value;
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

    public Color GetColour(string key) => GetColour(key, Color.Empty);

    public Color GetColour(string key, Color defaultValue) => TryParse(key, Color.FromName, defaultValue);

    public int GetInt(string key) => GetInt(key, 0);

    public int GetInt(string key, int defaultValue) => TryParse(key, int.Parse, 0);

    public bool GetBool(string key) => GetBool(key, false);

    public bool GetBool(string key, bool defaultValue) => TryParse(key, bool.Parse, false);

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
