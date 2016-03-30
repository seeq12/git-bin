using System;
using System.IO;
using System.Linq;
using GitBin.Remotes;

namespace GitBin.Commands
{
    /// <summary>
    /// Used to push chunks in the users local cache to S3
    /// </summary>
    public class PushCommand : ICommand
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRemote _remote;

        /// <param name="cacheManager">
        /// Manages the local cache and provides a set of methods to interface with the local cahce.
        /// </param>
        /// <param name="remote">Provides a set of tools to interface with the remote cache.</param>
        /// <param name="args">Arguments passed from the terminal when executed (there should not be any).</param>
        public PushCommand(
            ICacheManager cacheManager,
            IRemote remote,
            string[] args)
        {
            if (args.Length > 0)
                throw new ArgumentException();

            _cacheManager = cacheManager;
            _remote = remote;
        }

        /// <summary>
        /// Decides what files are in the local cache but not in the remote cache, calls the AysncFileProcessor to 
        /// uploads the files and verifies that the chunk is actaully correct.
        /// </summary>
        public void Execute()
        {
            var filesInRemote = _remote.ListFiles();
            var filesInCache = _cacheManager.ListCachedChunks();

            var filesToUpload = filesInCache.Except(filesInRemote).Select(x => x.Name).ToArray();

            if (filesToUpload.Length == 0)
            {
                GitBinConsole.WriteLineNoPrefix("All chunks already present on remote");
            }
            else
            {
                if (filesToUpload.Length == 1)
                {
                    GitBinConsole.Write("Uploading 1 chunk: ");
                }
                else
                {
                    GitBinConsole.Write("Uploading {0} chunks: ", filesToUpload.Length);
                }

                try
                {
                    AsyncFileProcessor.ProcessFiles(filesToUpload, 1, (chunkHash, progressListener) =>
                    {
                        verifyChunkIntegrity(chunkHash);
                        _remote.UploadFile(_cacheManager.GetPathForChunk(chunkHash), chunkHash, progressListener);
                    });
                }
                catch (InvalidDataException e)
                {
                    throw new ಠ_ಠ(e.Message);
                }
                catch (ಠ_ಠ e)
                {
                    throw new ಠ_ಠ(String.Format("Encountered an error uploading chunk: {0}", e.Message));
                }
            }
        }

        /// <summary>
        /// Checks to ensure the integrity of the data by reading the file contents, caculating its hash, and comparing
        /// it to the provided hash.
        /// </summary>
        /// <param name="chunkHash">Hash to be verified</param>
        private void verifyChunkIntegrity(string chunkHash)
        {
            var chunkPath = _cacheManager.GetPathForChunk(chunkHash);
            byte[] chunkData = File.ReadAllBytes(chunkPath);
            var calculatedChunkHash = CacheManager.GetHashForChunk(chunkData, chunkData.Length);

            if (!calculatedChunkHash.Equals(chunkHash))
            {
                throw new InvalidDataException("Chunk '" + chunkHash + "' is corrupted in the local cache, aborting.");
            }
        }
    }
}