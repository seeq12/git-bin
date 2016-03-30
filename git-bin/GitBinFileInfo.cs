using System;

namespace GitBin
{
    /// <summary>
    /// Representation of a file, including its name and size.
    /// </summary>
    public class GitBinFileInfo
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Size in bytes of the file.
        /// </summary>
        public long SizeInBytes { get; private set; }

        /// <param name="name">Name of the file.</param>
        /// <param name="size">Size in bytes of the file.</param>
        public GitBinFileInfo(string name, long sizeInBytes)
        {
            this.Name = name;
            this.SizeInBytes = sizeInBytes;
        }

        public bool Equals(GitBinFileInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && other.SizeInBytes == SizeInBytes;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(GitBinFileInfo)) return false;
            return Equals((GitBinFileInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ SizeInBytes.GetHashCode();
            }
        }
    }
}