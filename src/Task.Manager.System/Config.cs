using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System;

public class Config
{
    public static Config FromFile(IFileSystem fileSys, string path)
    {
        if (fileSys == null) throw new ArgumentNullException(nameof(fileSys));
        if (path == null) throw new ArgumentNullException(nameof(path));

        if (false == fileSys.Exists(path)) {
            throw new Exception();
        }

        string buf = fileSys.ReadAllText(path);
        return FromStringInternal(buf);
    }

    private static Config FromStringInternal(string buf)
    {
        var config = new Config();
        return config;
    }
}