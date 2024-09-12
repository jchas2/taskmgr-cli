using System.Text;

namespace Task.Manager.System.Configuration;

public sealed class ConfigParser
{
    public void Parse(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        string[] lines = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        ParseLines(lines);
    }

    private void ParseLines(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++) {
            ParseLine(lines[i]);
        }
    }

    private void ParseLine(in string line)
    {
        for (int ptr = 0; ptr < line.Length; ptr++) {
            char ch = line[ptr];

            if (ch == '#' || ch == ';') {
                return;
            }

            if (ch == '[') {
                string? sectionName = GetSectionIdentifier(line, ++ptr);
                if (string.IsNullOrWhiteSpace(sectionName)) {

                }
            }

            if(char.IsWhiteSpace(ch)) {
                
            }
        }
    }

    private string? GetSectionIdentifier(in string line, int ptr)
    {
        var buf = new StringBuilder();

        for (; ; ) {
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

            ptr++;
        }
    }
}
