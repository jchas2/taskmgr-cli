namespace Task.Manager.Commands;

public abstract class AbstractCommand(string text) : ICommand
{
    public virtual void Execute() => throw new NotImplementedException();
    public virtual bool IsEnabled { get; } = false;
    public string Text => text;
}

