using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System;

public class Config
{
    public static Config FromFile(IFileSystem fileSys, string path)
    {
        ArgumentNullException.ThrowIfNull(fileSys);
        ArgumentNullException.ThrowIfNull(path);

        if (false == fileSys.Exists(path)) {
            throw new Exception();
        }

        string buf = fileSys.ReadAllText(path);
        return FromStringInternal(buf);
    }

    // Use a ConfigLoadException, ConfigParseException, derive from ConfigException for better error handling at the higher levels.
    
    private static Config FromStringInternal(string buf)
    {
        var config = new Config();
        return config;
    }
}