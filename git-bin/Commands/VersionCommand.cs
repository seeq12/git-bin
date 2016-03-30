using System;
using System.Reflection;

namespace GitBin.Commands
{
    /// <summary>
    /// Used to find the version number of git-bin
    /// </summary>
    public class VersionCommand : ICommand
    {
        /// <summary>
        /// Writes to console the version number of git-bin
        /// </summary>
        public void Execute()
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}