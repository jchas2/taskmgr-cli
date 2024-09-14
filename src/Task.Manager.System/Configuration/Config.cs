using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System.Configuration;

public class Config
{
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
}