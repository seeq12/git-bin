using System;

namespace GitBin.Commands
{
    /// <summary>
    /// Used to show the user what command options are present.
    /// </summary>
    public class ShowUsageCommand : ICommand
    {
        /// <summary>
        /// Displays the git bin operations that can be executed.
        /// </summary>
        public void Execute()
        {
            GitBinConsole.WriteLineNoPrefix("usage: git bin [--version]");
            GitBinConsole.WriteLineNoPrefix("               <command> [<args>]");
            GitBinConsole.WriteNoPrefix(Environment.NewLine);
            GitBinConsole.WriteLineNoPrefix("List of available commands:");
            GitBinConsole.WriteLineNoPrefix("  clean    Clean filter. Should only be used with .gitattributes filtering");
            GitBinConsole.WriteLineNoPrefix("  clear    Remove all files in the local cache directory. Requires -n or -f");
            GitBinConsole.WriteLineNoPrefix("  push     Upload changed files to the remote file repository");
            GitBinConsole.WriteLineNoPrefix("  smudge   Smudge filter. Should only be used with .gitattributes filtering");
            GitBinConsole.WriteLineNoPrefix("  status   Display status of the local cache. [-r], include the remote repo");
        }
    }
}