using System;
using System.Collections.Generic;
using System.Linq;
using GitBin.Remotes;

namespace GitBin.Commands
{
    /// <summary>
    /// Used to check the status of the local cache and remote cache if desired.
    /// </summary>
    public class StatusCommand : ICommand
    {
        public const string ShowRemoteArgument = "-r";

        private readonly ICacheManager _cacheManager;
        private readonly IRemote _remote;
        private readonly bool _shouldShowRemote;
        private readonly GitBinFileInfo[] _filesInLocalCache;

        /// <param name="cacheManager">
        /// Manages the local cache and provides a set of methods to interface with the local cahce.
        /// </param>
        /// <param name="remote">Provides a set of tools to interface with the remote cache.</param>
        /// <param name="args">
        /// Arguement passed from the console describing whether or not to show the status of the remote bucket
        /// </param>
        public StatusCommand(
            ICacheManager cacheManager,
            IRemote remote,
            string[] args)
        {
            if (args.Length > 1)
                throw new ArgumentException();

            if (args.Length == 1)
            {
                if (args[0] == ShowRemoteArgument)
                {
                    _shouldShowRemote = true;
                }
                else
                {
                    throw new ArgumentException("status command only has one valid option: " + ShowRemoteArgument);
                }
            }

            _cacheManager = cacheManager;
            _remote = remote;

            _filesInLocalCache = _cacheManager.ListCachedChunks();
        }

        /// <summary>
        /// Prints the status about the local cache and if desired prints the status of the remote as well.
        /// </summary>
        public void Execute()
        {
            PrintStatusAboutCache();

            if (_shouldShowRemote)
            {
                PrintStatusAboutRemote();
            }
        }

        /// <summary>
        /// Prints out the number of items present in the local cache and the size of the local cahce folder.
        /// </summary>
        private void PrintStatusAboutCache()
        {
            GitBinConsole.WriteLineNoPrefix("Local cache:");
            GitBinConsole.WriteLineNoPrefix("  items: {0}", _filesInLocalCache.Length);
            GitBinConsole.WriteLineNoPrefix("  size:  {0}", GitBinFileInfoUtils.GetHumanReadableSize(_filesInLocalCache));
        }

        /// <summary>
        /// Prints out the number of items present in the remote and the size of those items. Also prints out how many 
        /// items there are to push to the remote and the size of those items.
        /// </summary>
        private void PrintStatusAboutRemote()
        {
            var remoteFiles = _remote.ListFiles();

            GitBinConsole.WriteLineNoPrefix("\nRemote repo:");
            GitBinConsole.WriteLineNoPrefix("  items: {0}", remoteFiles.Length);
            GitBinConsole.WriteLineNoPrefix("  size:  {0}", GitBinFileInfoUtils.GetHumanReadableSize(remoteFiles));

            var filesToPush = _filesInLocalCache.Except(remoteFiles).ToList();

            GitBinConsole.WriteLineNoPrefix("\nTo push:");
            GitBinConsole.WriteLineNoPrefix("  items: {0}", filesToPush.Count);
            GitBinConsole.WriteLineNoPrefix("  size:  {0}", GitBinFileInfoUtils.GetHumanReadableSize(filesToPush));
        }
    }
}