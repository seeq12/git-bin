using System;

namespace GitBin.Commands
{
    /// <summary>
    /// Used to clear the local cache 
    /// </summary>
    public class ClearCommand : ICommand
    {
        public const string DryRunArgument = "-n";
        public const string ForceArgument = "-f";

        private readonly ICacheManager _cacheManager;
        private readonly bool _isDryRun;

        /// <param name="cacheManager">
        /// Manages the local cache and provides a set of methods to interface with the local cahce
        /// </param>
        /// <param name="args">Argument passed in from the console either as a DryRun flag or a Force flag.</param>
        public ClearCommand(
            ICacheManager cacheManager,
            string[] args)
        {
            if (!TryParseArguments(args, out _isDryRun))
                throw new ArgumentException(string.Format("clear command requires either {0} or {1}", DryRunArgument, ForceArgument));

            _cacheManager = cacheManager;
        }

        /// <summary>
        /// If dry run is true, prompt the user to let them know what would have been cleared out of the cache, else 
        /// purge the contents from the local cache.
        /// </summary>
        public void Execute()
        {
            if (_isDryRun)
            {
                GitBinConsole.WriteLine("clear dry run: would remove " +
                    GitBinFileInfoUtils.GetHumanReadableSize(_cacheManager.ListCachedChunks()));
            }
            else
            {
                _cacheManager.ClearCache();
            }
        }

        /// <summary>
        /// Attempts to figure out if Force or DryRun argument is passed from the console. If the Force argument is 
        /// present then it will set isDryRun to be false.
        /// </summary>
        /// <param name="args">Arguments passed from the console (sould only be 1 argument).</param>
        /// <param name="isDryRun">Boolean describing if DryRun is currently true or not.</param>
        /// <returns>Whether or not a DryRun or Force argument is provided.</returns>
        private bool TryParseArguments(string[] args, out bool isDryRun)
        {
            isDryRun = true;

            if (args.Length != 1)
                return false;

            switch (args[0])
            {
                case DryRunArgument:
                    return true;

                case ForceArgument:
                    isDryRun = false;
                    return true;

                default:
                    return false;
            }
        }
    }
}