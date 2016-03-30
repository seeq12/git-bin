using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace GitBin
{
    /// <summary>
    /// Enables the storage and retrieval of chunks to a cache.
    /// </summary>
    public interface ICacheManager
    {
        /// <summary>
        /// Retrieve a chunk from the cache.
        /// </summary>
        /// <param name="chunkHash">Hash of the chunk that is desired.</param>
        /// <returns>Contents of the chunk.</returns>
        byte[] ReadChunkFromCache(string chunkHash);

        /// <summary>
        /// Write a chunk to the local cache for future (and current) use.
        /// </summary>
        /// <param name="contents">Contents of the chunk.</param>
        /// <param name="contentLength">Length of the provided buffer to consider as part of the chunk.</param>
        /// <returns>Hash of the chunk that was cached.</returns>
        string WriteChunkToCache(byte[] contents, int contentLength);

        /// <summary>
        /// Retrieves a list of chunks that are in the cache.
        /// </summary>
        /// <returns>List of chunks that are in the cache.</returns>
        GitBinFileInfo[] ListCachedChunks();

        /// <summary>
        /// Delete all chunks from the cache.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Given a list of chunks, determine which are not yet in the cache.
        /// </summary>
        /// <param name="chunkHashes">List of chunks that may or may not be in the cache.</param>
        /// <returns>List of chunks that are in chunkHashes but not the local cache.</returns>
        string[] GetChunksNotInCache(IEnumerable<string> chunkHashes);

        /// <summary>
        /// Retrieve the file path for a given chunk hash.
        /// </summary>
        /// <param name="chunkHash">Hash of a chunk.</param>
        /// <returns>File path for a given chunk hash.</returns>
        string GetPathForChunk(string chunkHash);
    }

    public class CacheManager : ICacheManager
    {
        private readonly DirectoryInfo _cacheDirectoryInfo;

        public CacheManager(IConfigurationProvider configurationProvider)
        {
            _cacheDirectoryInfo = Directory.CreateDirectory(configurationProvider.CacheDirectory);
        }

        public byte[] ReadChunkFromCache(string chunkHash)
        {
            var path = GetPathForChunk(chunkHash);

            if (!File.Exists(path))
            {
                throw new ಠ_ಠ("Tried to read chunk from cache that does not exist. [" + chunkHash + ']');
            }

            byte[] chunkContents = File.ReadAllBytes(path);
            string computedChunkHash = GetHashForChunk(chunkContents, chunkContents.Length);

            if (!computedChunkHash.Equals(chunkHash))
            {
                throw new ಠ_ಠ("Chunk '" + chunkHash + "' is corrupted in the local cache, aborting.");
            }

            return chunkContents;
        }

        public string WriteChunkToCache(byte[] contents, int contentLength)
        {
            var chunkHash = GetHashForChunk(contents, contentLength);
            var path = GetPathForChunk(chunkHash);

            if (File.Exists(path))
                return chunkHash;

            var filestream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, contentLength, FileOptions.WriteThrough);
            filestream.Write(contents, 0, contentLength);
            filestream.Close();

            return chunkHash;
        }

        public GitBinFileInfo[] ListCachedChunks()
        {
            var allFiles = _cacheDirectoryInfo.GetFiles();
            var gitBinFileInfos = allFiles.Select(fi => new GitBinFileInfo(fi.Name, fi.Length));

            // Try to remove any items from the list that clearly are not chunks, such as those files that don't have
            // exactly 64 characters in their name.
            gitBinFileInfos = gitBinFileInfos.Where(info => info.Name.Length == 32 * 2);

            return gitBinFileInfos.ToArray();
        }

        public void ClearCache()
        {
            foreach (var file in ListCachedChunks())
            {
                File.Delete(GetPathForChunk(file.Name));
            }
        }

        public string[] GetChunksNotInCache(IEnumerable<string> chunkHashes)
        {
            var chunksInCache = ListCachedChunks().Select(fi => fi.Name);
            var chunksNotInCache = chunkHashes.Except(chunksInCache);

            return chunksNotInCache.ToArray();
        }

        public string GetPathForChunk(string chunkHash)
        {
            return Path.Combine(_cacheDirectoryInfo.FullName, chunkHash);
        }

        /// <summary>
        /// Compute the hash for a chunk.
        /// </summary>
        /// <param name="chunkBuffer">Chunk data to hash.</param>
        /// <param name="chunkLength">0 based length to hash in the provided chunkBuffer.</param>
        /// <returns>Hash for the provided chunkBuffer.</returns>
        public static string GetHashForChunk(byte[] chunkBuffer, int chunkLength)
        {
            var hasher = new SHA256Managed();

            var hashBytes = hasher.ComputeHash(chunkBuffer, 0, chunkLength);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", String.Empty);

            return hashString;
        }
    }
}