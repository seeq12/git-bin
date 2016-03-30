using System;
using System.Diagnostics;

namespace GitBin
{
    /// <summary>
    /// Runs a git instance and returns its stdout
    /// </summary>
    public interface IGitExecutor
    {
        /// <summary>
        /// Runs a git instance and returns the stdout parsed into a long.
        /// </summary>
        /// <param name="arguments">Command line arguements to pass to the git instance.</param>
        /// <returns>Git's stdout parsed into a long</returns>
        long? GetLong(string arguments);

        /// <summary>
        /// Runs a git instance and returns the trimmed stdout.
        /// </summary>
        /// <param name="arguments">Command line arguements to pass to the git instance.</param>
        /// <returns>Git's trimmed stdout.</returns>
        string GetString(string arguments);
    }

    /// <summary>
    /// A git executor that actually invokes a git process to fullfill requests.
    /// </summary>
    public class GitExecutor : IGitExecutor
    {
        private const int Success = 0;
        private const int MissingSectionOrKey = 1;

        /// <summary>
        /// <see cref="IGitExecutor.GetLong(string)"/>
        /// </summary>
        public long? GetLong(string arguments)
        {
            var rawValue = ExecuteGit(arguments);

            if (string.IsNullOrEmpty(rawValue))
                return null;

            return Convert.ToInt64(rawValue);
        }

        /// <summary>
        /// <see cref="IGitExecutor.GetString(string)"/>
        /// </summary>
        public string GetString(string arguments)
        {
            return ExecuteGit(arguments);
        }

        /// <summary>
        /// Spawns a git instance and captures its stdout.
        /// </summary>
        /// <param name="arguments">Arguements to pass to the spawned git instance.</param>
        /// <returns>Trimmed stdout of the git process.</returns>
        private static string ExecuteGit(string arguments)
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = arguments;
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            switch (process.ExitCode)
            {
                case Success:
                case MissingSectionOrKey:
                    return output.Trim();

                default:
                    throw new ಠ_ಠ("git exited with error code [" + process.ExitCode + "] while executing command [" + arguments + ']');
            }
        }
    }
}