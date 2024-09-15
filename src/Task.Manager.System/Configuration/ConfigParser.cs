using System.Collections.ObjectModel;
using System.Text;
using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System.Configuration;

public class ConfigParser : IDisposable
{
    private const int EndOfFile = -1;
    private const int InitialStringSize = 32;
    private readonly TextReader _reader;
    private readonly IList<ConfigSection> _sections;
    
    public ConfigParser(string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        _reader = new StringReader(str);
        _sections = new List<ConfigSection>();
    }

    public ConfigParser(IFileSystem fileSys, string path)
    {
        ArgumentNullException.ThrowIfNull(fileSys);
        ArgumentNullException.ThrowIfNull(path);

        if (false == fileSys.Exists(path)) {
            throw new FileNotFoundException(path);
        }

        _reader = new StreamReader(path);
        _sections = new List<ConfigSection>();
    }

    ~ConfigParser() => 
        Dispose();
    
    public void Dispose() =>
        _reader.Dispose();

    public bool Parse()
    {
        bool result = true;
        bool inComment = false;
        ConfigSection? section = null;

        for (;;) {
            int ch = _reader.Read();

            if (ch == EndOfFile) {
                break;
            }

            char c = (char)ch;

            if (c == '\n') {
                inComment = false;
                continue;
            }

            if (inComment) {
                continue;
            }
            
            if (char.IsWhiteSpace(c)) {
                continue;
            }

            if (c == '#' || c == ';') {
                inComment = true;
                continue;
            }

            if (c == '[') {
                section = new ConfigSection();

                if (false == ParseSection(section)) {
                    return false;
                }
                
                _sections.Add(section);
                continue;
            }

            if (false == char.IsLetter(c)) {
                break;
            }

            if (section != null && ParseKey(section, c)) {
                continue;
            }

            if (section == null) {
                break;
            }
        }

        return result;
    }
    
    private bool ParseSection(ConfigSection section)
    {
        var buf = new StringBuilder(InitialStringSize);
        
        for (;;) {
            int ch = _reader.Read();
            
            if (ch == EndOfFile) {
                return false;
            }

            char c = (char)ch;

            if (c == ']') {
                break;
            }

            if (char.IsWhiteSpace(c)) {
                return false;
            }

            if (false == char.IsLetterOrDigit(c)) {
                return false;
            }

            buf.Append(c);
        }

        if (buf.Length == 0) {
            return false;
        }
        
        section.Name = buf.ToString();
        return true;
    }

    private bool ParseKey(ConfigSection section, char initChar)
    {
        var keyBuf = new StringBuilder(InitialStringSize);
        keyBuf.Append(initChar);
        
        int ch;
        char c;
        
        for (;;) {
            ch = _reader.Read();
            
            if (ch == EndOfFile) {
                return false;
            }

            c = (char)ch;

            if (false == (char.IsLetterOrDigit(c) || c == '-')) {
                break;
            }

            keyBuf.Append(c);
        }

        while (c == ' ' || c == '\t') {
            c = (char)(_reader.Read());
        }

        if (c != '=') {
            return false;
        }

        var valBuf = new StringBuilder(InitialStringSize);

        if (false == ParseValue(valBuf)) {
            return false;
        }

        if (keyBuf.Length == 0 || valBuf.Length == 0) {
            return false;
        }
        
        string key = keyBuf.ToString().ToLower();
        string val = valBuf.ToString();

        if (false == section.Contains(key)) {
            section.Add(key, val);
        }

        return true;
    }

    private bool ParseValue(StringBuilder valBuf)
    {
        bool inComment = false;
        
        for (;;) {
            char c = (char)_reader.Read();

            if (c == '\n') {
                return valBuf.Length > 0;
            }

            if (inComment) {
                continue;
            }

            if (char.IsWhiteSpace(c)) {
                valBuf.Append(c);
                continue;
            }

            if (c == '#' || c == ';') {
                inComment = true;
                continue;
            }

            valBuf.Append(c);
        }
    }

    public IList<ConfigSection> Sections => _sections;
}
