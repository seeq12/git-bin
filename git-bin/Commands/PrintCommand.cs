namespace GitBin.Commands
{
    /// <summary>
    /// Used to print out a message.
    /// </summary>
    public class PrintCommand : ICommand
    {
        private readonly string _message;

        /// <param name="message">Message provided to the printer.</param>
        public PrintCommand(string message)
        {
            _message = message;
        }

        /// <summary>
        /// Writes message out to the console.
        /// </summary>
        public void Execute()
        {
            GitBinConsole.WriteLine(_message);
        }
    }
}