using System;
using System.Collections.Generic;
using System.IO;

namespace GitBin.Remotes
{
    /// <summary>
    /// Interface for storing and retrieving files remotely.
    /// </summary>
    public interface IRemote
    {
        GitBinFileInfo[] ListFiles();

        /// <summary>
        /// Uploads the files to the remote.
        /// </summary>
        /// <param name="sourceFilePath">Path to the file that is uploaded.</param>
        /// <param name="destinationFileName">Destination where the file will be uploaded to.</param>
        /// <param name="progressListener">
        /// An entity that istens for progress events from the AsyncFileProcessor and prints it out to the console.
        /// </param>
        void UploadFile(string sourceFilePath, string destinationFileName, Action<int> progressListener);

        /// <summary>
        /// Downloads the files from the remote.
        /// </summary>
        /// <param name="fileName">
        /// Name of the file to be downloaded, generally it is the hash of the file contents.
        /// </param>
        /// <param name="progressListener">
        /// An entity that istens for progress events from the AsyncFileProcesso and prints it out to the console.
        /// </param>
        /// <returns>File contents.</returns>
        byte[] DownloadFile(string fileName, Action<int> progressListener);
    }
}