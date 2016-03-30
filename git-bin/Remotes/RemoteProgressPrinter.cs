using System;
using System.Collections.Generic;
using System.Linq;

namespace GitBin.Remotes
{
    /// <summary>
    /// Prints progress percentage to the console.
    /// </summary>
    public class RemoteProgressPrinter : IDisposable
    {
        private readonly int _fileCount;
        private readonly int[] mostRecentFilePercentages;
        private int summedFilePercentage = 0;

        private string lastPrintedStatusString = "";

        /// <param name="chunkCount">The number of files that this progress printer will be tracking.</param>
        public RemoteProgressPrinter(int fileCount)
        {
            _fileCount = fileCount;
            mostRecentFilePercentages = Enumerable.Repeat(0, fileCount).ToArray();
        }

        public void Dispose()
        {
            GitBinConsole.WriteNoPrefix(Environment.NewLine);
        }

        /// <summary>
        /// Method that should be called when progress changes for a file. Prints the total percentage (for all files)
        /// to the console.
        /// </summary>
        /// <param name="fileNumber">
        /// The number (from 0 to the count provided in the constructor) of the file whose
        /// progress has changed.
        /// </param>
        /// <param name="percentageComplete">Percentage complete (0-100).</param>
        public void OnProgressChanged(int fileNumber, int percentageComplete)
        {
            lock (this)
            {
                var chunkProgressDelta = percentageComplete - mostRecentFilePercentages[fileNumber];
                mostRecentFilePercentages[fileNumber] = percentageComplete;
                summedFilePercentage += chunkProgressDelta;
                double totalPercentage = (double)summedFilePercentage / _fileCount;

                string percentageToPrint = String.Format("{0:N2}% ", totalPercentage);

                GitBinConsole.WriteNoPrefix(new String('\b', lastPrintedStatusString.Length) + percentageToPrint);
                GitBinConsole.Flush();

                lastPrintedStatusString = percentageToPrint;
            }

        }
    }
}