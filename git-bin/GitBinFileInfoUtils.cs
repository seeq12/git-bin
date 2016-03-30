using System;
using System.Collections.Generic;
using System.Linq;

namespace GitBin
{
    /// <summary>
    /// Helper class for Git Bin files. Tools used to work with the Git Bin file information.
    /// </summary>
    public static class GitBinFileInfoUtils
    {
        private static string[] Suffixes = new[] { "B", "k", "M", "G", "T", "P", "E" };

        /// <summary>
        /// Returns a readable string of file information.
        /// </summary>
        /// <param name="fileInfos">Collection of file information.</param>
        /// <returns>A string of file information that is readable.</returns>
        public static string GetHumanReadableSize(IEnumerable<GitBinFileInfo> fileInfos)
        {
            var totalSize = fileInfos.Sum(fi => fi.SizeInBytes);

            return GetHumanReadableSize(totalSize);
        }

        /// <summary>
        /// Helper method.
        /// </summary>
        /// <param name="numberOfBytes">Size of the information in bytes.</param>
        /// <returns>A string of file information that is readable.</returns>
        public static string GetHumanReadableSize(long numberOfBytes)
        {
            int suffixIndex = 0;
            int increment = 1024;
            double scaledNumberOfBytes = numberOfBytes;

            if (numberOfBytes > 0)
            {
                while (scaledNumberOfBytes >= increment)
                {
                    suffixIndex++;
                    scaledNumberOfBytes /= increment;
                }

                if (Math.Abs(scaledNumberOfBytes - 0) < 0.1)
                {
                    scaledNumberOfBytes = 1;
                }
            }

            return String.Format("{0}{1}", scaledNumberOfBytes.ToString("0.#"), Suffixes[suffixIndex]);
        }
    }
}