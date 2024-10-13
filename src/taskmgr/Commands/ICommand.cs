namespace Task.Manager.Commands;

public interface ICommand
{
    void Execute();
    bool IsEnabled { get; }
}

