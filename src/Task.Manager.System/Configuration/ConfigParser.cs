using System.Collections.ObjectModel;
using System.Diagnostics;
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

    public void Parse()
    {
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
                ParseSection(ref section);
                _sections.Add(section);
                continue;
            }

            if (false == char.IsLetter(c)) {
                break;
            }

            if (section != null) {
                ParseKey(ref section, c);
                continue;
            }

            if (section == null) {
                break;
            }
        }
    }
    
    private void ParseSection(ref ConfigSection section)
    {
        var buf = new StringBuilder(InitialStringSize);
        
        for (;;) {
            int ch = _reader.Read();
            char c = (char)ch;
            
            if (ch == EndOfFile) {
                buf.Append(c);
                throw new ConfigParseException($"End-of-file found reading section name {buf} before closing ']'.");
            }

            if (c == ']') {
                break;
            }

            if (char.IsWhiteSpace(c)) {
                buf.Append(c);
                throw new ConfigParseException($"Unexpected white space char in section name {buf}.");
            }

            if (false == (char.IsLetterOrDigit(c) || c == '-')) {
                buf.Append(c);
                throw new ConfigParseException($"Unexpected char in section name {buf}. Must be alpha-numeric.");
            }

            buf.Append(c);
        }

        if (buf.Length == 0) {
            throw new ConfigParseException("Section name cannot be empty.");
        }
        
        section.Name = buf.ToString();
    }

    private void ParseKey(ref ConfigSection section, char initChar)
    {
        var keyBuf = new StringBuilder(InitialStringSize);
        keyBuf.Append(initChar);

        char c;
        
        for (;;) {
            var ch = _reader.Read();
            c = (char)ch;
            
            if (ch == EndOfFile) {
                keyBuf.Append(c);
                throw new ConfigParseException($"End-of-file found reading key name {keyBuf}.");
            }
            
            if (false == (char.IsLetterOrDigit(c) || c == '-')) {
                break;
            }

            keyBuf.Append(c);
        }

        while (c == ' ' || c == '\t') {
            c = (char)(_reader.Read());
        }

        if (c != '=') {
            throw new ConfigParseException($"Expected '=' after key {keyBuf}.");
        }

        /* Keys are mandatory, values are not. */
        if (keyBuf.Length == 0) {
            throw new ConfigParseException("Key name cannot be empty.");
        }

        var valBuf = new StringBuilder(InitialStringSize);
        ParseValue(ref valBuf);

        string key = keyBuf.ToString().ToLower();
        string val = valBuf.ToString();

        if (false == section.Contains(key)) {
            section.Add(key, val);
        }
    }

    private void ParseValue(ref StringBuilder valBuf)
    {
        bool inComment = false;
        int ch;
        
        for (;;) {
            ch = _reader.Read();

            if (ch == EndOfFile) {
                return;
            }
            
            char c = (char)ch;
            
            if (c == '\n') {
                return;
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
