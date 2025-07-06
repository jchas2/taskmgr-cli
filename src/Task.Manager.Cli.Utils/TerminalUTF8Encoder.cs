using System.Security;
using System.Text;

namespace Task.Manager.Cli.Utils;

public sealed class TerminalUTF8Encoder : IDisposable
{
    private Encoding? _origOutputEncoding;
    private Encoding? _origInputEncoding;

    public TerminalUTF8Encoder()
    {
        UseTryCatch(() => {
            _origOutputEncoding = Console.OutputEncoding;
            _origInputEncoding = Console.InputEncoding;
        });
    }

    public void Dispose()
    {
        UseTryCatch(() => {
            Console.OutputEncoding = _origOutputEncoding ?? Console.OutputEncoding;
            Console.InputEncoding = _origInputEncoding ?? Console.InputEncoding;
        });
    }

    private void UseTryCatch(Action action)
    {
        try {
            action();
        }
        catch (Exception e) when (e is IOException || e is SecurityException) {
            // TODO: Print nice exception using framework.
            Console.WriteLine(e);
        }
    }
}
