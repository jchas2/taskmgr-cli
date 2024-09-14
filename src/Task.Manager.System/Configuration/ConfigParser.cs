using System.Collections.ObjectModel;
using System.Text;
using Task.Manager.Internal.Abstractions;

namespace Task.Manager.System.Configuration;

public class ConfigParser : IDisposable
{
    private const int EndOfFile = -1;
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
        ArgumentNullException.ThrowIfNull(path);

        if (false == fileSys.Exists(path)) {
            throw new InvalidOperationException();
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

            if (false == char.IsLetterOrDigit(c)) {
                break;
            }
            
        }

        return result;
    }
    
    private bool ParseSection(ConfigSection configSection)
    {
        var buf = new StringBuilder();
        
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
        
        configSection.Name = buf.ToString();
        return true;
    }

    public IList<ConfigSection> Sections => _sections;
}
