namespace GitBin.Commands
{
    /// <summary>
    /// Interface for all the commands, requires an Execute method.
    /// </summary>
    public interface ICommand
    {
        void Execute();
    }
}