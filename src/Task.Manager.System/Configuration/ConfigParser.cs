using System.Text;

namespace Task.Manager.System.Configuration;

public sealed class ConfigParser
{
    public IList<ConfigSection> Parse(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        string[] lines = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        ParseLines(lines);
        return new List<ConfigSection>();
    }

    private void ParseLines(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++) {
            ParseLine(lines[i]);
        }
    }

    private void ParseLine(string line)
    {
        for (int ptr = 0; ptr < line.Length; ptr++) {
            char ch = line[ptr];

            if (ch == '#' || ch == ';') {
                return;
            }

            if (ch == '[') {
                string? section = GetSectionIdentifier(line, ++ptr);
                if (string.IsNullOrWhiteSpace(section)) {

                }
            }

            if(char.IsWhiteSpace(ch)) {
                
            }
        }
    }

    private string? GetSectionIdentifier(string line, int ptr)
    {
        var buf = new StringBuilder();

        for ( ;; ) {
            if (ptr >= line.Length) {
                return null;
            }

            char ch = line[ptr];

            if (ch == ']') {
                return buf.ToString();
            }
            
            if (char.IsLetterOrDigit(ch)) {
                buf.Append(ch);
            }
            else if (ch == '-') {
                buf.Append(ch);
            }

            ptr++;
        }
    }

    private (string? key, string? val) GetKeyValue(string line, int ptr)
    {
        var keyBuf = new StringBuilder();
        var valBuf = new StringBuilder();

        for (;;) {
            if (ptr >= line.Length) {
                return (null, null);
            }

            char ch = line[ptr];

            ptr++;
        }

        //return (keyBuf.ToString(), valBuf.ToString());
    } 
}
