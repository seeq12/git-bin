using System;
using System.Collections.Generic;
using System.IO;
using GitBin.Remotes;

namespace GitBin.Commands
{
    /// <summary>
    /// Used to read in the yaml file and download missing chunks when a git clone or pull is done.
    /// </summary>
    public class SmudgeCommand : ICommand
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRemote _remote;

        /// <param name="cacheManager">
        /// Manages the local cache and provides a set of methods to interface with the local cahce.
        /// </param>
        /// <param name="remote">Provides a set of tools to interface with the remote cache.</param>
        /// <param name="args">Arguments passed from the console (there should not be any).</param>
        public SmudgeCommand(
            ICacheManager cacheManager,
            IRemote remote,
            string[] args)
        {
            if (args.Length != 0)
                throw new ArgumentException();

            _cacheManager = cacheManager;
            _remote = remote;
        }

        /// <summary>
        /// Reads the git Yaml file and downloads the chunks that are not present in the local cache but is listed on
        /// the Yaml file.
        /// </summary>
        public void Execute()
        {
            var stdin = Console.OpenStandardInput();
            var document = GitBinDocument.FromYaml(new StreamReader(stdin));

            GitBinConsole.Write("Smudging {0}:", document.Filename);

            DownloadMissingChunks(document.ChunkHashes);
            OutputReassembledChunks(document.ChunkHashes);
        }

        /// <summary>
        /// Figures out what chunks are missing and downloads those chunks by using the AysncFileProcessor with the 
        /// DownloadChunk action.
        /// </summary>
        /// <param name="chunkHashes"></param>
        private void DownloadMissingChunks(IEnumerable<string> chunkHashes)
        {
            string[] chunksToDownload = _cacheManager.GetChunksNotInCache(chunkHashes);

            if (chunksToDownload.Length == 0)
            {
                GitBinConsole.WriteLineNoPrefix(" All chunks already present in cache");
            }
            else
            {
                if (chunksToDownload.Length == 1)
                {
                    GitBinConsole.WriteNoPrefix(" Downloading 1 chunk: ");
                }
                else
                {
                    GitBinConsole.WriteNoPrefix(" Downloading {0} chunks: ", chunksToDownload.Length);
                }

                try
                {
                    AsyncFileProcessor.ProcessFiles(chunksToDownload, DownloadChunk);
                }
                catch (ಠ_ಠ e)
                {
                    throw new ಠ_ಠ(String.Format("Encountered an error downloading chunk: {0}", e.Message));
                }
            }
        }

        /// <summary>
        /// Downloads the chunk from S3, compares the file content's hash to the hash provided to verify its integrity,
        /// and writes it to the local cache.
        /// </summary>
        /// <param name="chunkHash">The specified hash for the chunk to be downloaded.</param>
        /// <param name="progressListener">
        /// An entity that istens for progress events from the AsyncFileProcessor and prints it out to the console.
        /// </param>
        private void DownloadChunk(string chunkHash, Action<int> progressListener)
        {
            var fullPath = _cacheManager.GetPathForChunk(chunkHash);
            var chunkData = _remote.DownloadFile(chunkHash, progressListener);
            var computedChunkHash = CacheManager.GetHashForChunk(chunkData, chunkData.Length);

            // A chunk's name is its hash. If a download's name and hash don't match then try and download it
            // again, because it failed the first time.
            if (chunkHash.Equals(computedChunkHash))
            {
                _cacheManager.WriteChunkToCache(chunkData, chunkData.Length);
            }
            else
            {
                throw new ಠ_ಠ("Downloaded a corrupted chunk (" + chunkHash + ")");
            }
        }

        /// <summary>
        /// Reassembles the chunks and writes it out to the console.
        /// </summary>
        /// <param name="chunkHashes">A collection of chunks to be reassembled.</param>
        private void OutputReassembledChunks(IEnumerable<string> chunkHashes)
        {
            var stdout = Console.OpenStandardOutput();

            foreach (var chunkHash in chunkHashes)
            {
                var chunkData = _cacheManager.ReadChunkFromCache(chunkHash);
                stdout.Write(chunkData, 0, chunkData.Length);
            }

            stdout.Flush();
        }
    }
}