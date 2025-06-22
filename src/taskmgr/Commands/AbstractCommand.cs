namespace Task.Manager.Commands;

public abstract class AbstractCommand : ICommand
{
    public virtual void Execute() => throw new NotImplementedException();
    public virtual bool IsEnabled { get; } = false;
}

