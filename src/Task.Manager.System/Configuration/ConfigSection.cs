using System.Text;
using Task.Manager.Cli.Utils;

namespace Task.Manager.System.Configuration;

public sealed class ConfigSection
{
    private string name = string.Empty;
    private readonly Dictionary<string, string> keys;

    internal ConfigSection() => keys = new Dictionary<string, string>();

    public ConfigSection(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        this.name = name;
        keys = new Dictionary<string, string>();
    }
    
    public ConfigSection Add(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        keys[key] = value;

        return this;
    }

    public ConfigSection AddIfMissing(string key, string value)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        if (!keys.ContainsKey(key)) {
            Add(key, value);
        }

        return this;
    }

    public bool Contains(string key) => keys.ContainsKey(key);

    public string GetString(string key) => GetString(key, string.Empty);

    public string GetString(string key, string defaultValue)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(defaultValue);
        
        if (!Contains(key)) {
            return defaultValue;
        }

        return keys[key];
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
        get => name;
        set => name = value ?? throw new ArgumentNullException(nameof(Name));
    }

    public override string ToString()
    {
        StringBuilder buffer = new(1024);
        buffer.AppendLine($"[{name}]");

        foreach (KeyValuePair<string,string> pair in keys) {
            buffer.AppendLine($"{pair.Key}={pair.Value}");
        }
        
        return buffer.ToString();
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
