using System;
using System.Collections.Generic;
using System.IO;
using GitBin.Remotes;

namespace GitBin
{
    /// <summary>
    /// A single location to store all user-configurable options of this program.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Maximum size (in bytes) of a chunk.
        /// </summary>
        long ChunkSize { get; }

        /// <summary>
        /// Transport protocol to use when communication with S3. Either HTTPS or HTTP.
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Local directory to use for chunk storage.
        /// </summary>
        string CacheDirectory { get; }

        /// <summary>
        /// S3 system name that will be used when communicating with S3. Examples: "us-east-1", "us-west-2"
        /// </summary>
        string S3SystemName { get; }

        /// <summary>
        /// S3 bucket (folder) that will be used to store and retrieve chunks.
        /// </summary>
        string S3Bucket { get; }

        /// <summary>
        /// S3 key (username) that will be used to authenticate with S3.
        /// </summary>
        string S3Key { get; }

        /// <summary>
        /// S3 secret key (password) that will be used to authenticate with S3.
        /// </summary>
        string S3SecretKey { get; }
    }

    public class ConfigurationProvider : IConfigurationProvider
    {
        public const long DefaultChunkSize = 1024 * 1024;
        public const long DefaultMaximumCacheSize = long.MaxValue;
        public const string DefaultProtocol = "HTTPS";
        public const string DefaultCacheDirectory = "git-bin";
        public const string DefaultS3SystemName = "us-east-1";

        public const string SectionName = "git-bin";
        public const string ChunkSizeConfigName = "chunkSize";
        public const string MaximumCacheSizeConfigName = "maxCacheSize";
        public const string ProtocolName = "protocol";
        public const string CacheDirectoryConfigName = "cacheDirectory";
        public const string S3SystemConfigName = "s3SystemName";
        public const string S3BucketConfigName = "s3bucket";
        public const string S3KeyConfigName = "s3key";
        public const string S3SecretKeyConfigName = "s3secretKey";

        private readonly IGitExecutor _gitExecutor;
        private readonly Dictionary<string, string> _configurationOptions;

        public long ChunkSize { get; private set; }
        public long MaximumCacheSize { get; private set; }
        public string CacheDirectory { get; private set; }
        public string Protocol { get; private set; }
        public string S3SystemName { get; private set; }
        public string S3Bucket { get; private set; }
        public string S3Key { get; private set; }
        public string S3SecretKey { get; private set; }

        public ConfigurationProvider(IGitExecutor gitExecutor)
        {
            _gitExecutor = gitExecutor;

            _configurationOptions = GetConfigurationOptions();

            this.ChunkSize = GetLong(ChunkSizeConfigName, DefaultChunkSize);
            this.MaximumCacheSize = GetLong(MaximumCacheSizeConfigName, DefaultMaximumCacheSize);
            this.Protocol = GetString(ProtocolName, DefaultProtocol);
            this.CacheDirectory = GetCacheDirectory(GetString(CacheDirectoryConfigName, DefaultCacheDirectory));

            this.S3SystemName = GetString(S3SystemConfigName, DefaultS3SystemName);
            this.S3Bucket = GetString(S3BucketConfigName);
            this.S3Key = GetString(S3KeyConfigName);
            this.S3SecretKey = GetString(S3SecretKeyConfigName);
        }

        private Dictionary<string, string> GetConfigurationOptions()
        {
            var rawAllOptions = _gitExecutor
                .GetString("config --get-regexp " + SectionName)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            var options = new Dictionary<string, string>();

            foreach (var option in rawAllOptions)
            {
                var optionKeyValue = option.Split(' ');

                if (optionKeyValue.Length != 2)
                    throw new ಠ_ಠ("Invalid config option: " + option);

                var key = optionKeyValue[0]
                    .Substring(optionKeyValue[0].IndexOf('.') + 1)
                    .ToLowerInvariant();

                options[key] = optionKeyValue[1];
            }

            return options;
        }

        private long GetLong(string name, long defaultValue)
        {
            string rawValue;

            if (!_configurationOptions.TryGetValue(name.ToLowerInvariant(), out rawValue))
                return defaultValue;

            var convertedValue = Convert.ToInt64(rawValue);

            if (convertedValue < 0)
                throw new ಠ_ಠ(name + " cannot be negative");

            return convertedValue;
        }

        private string GetString(string name)
        {
            return GetString(name, null);
        }

        private string GetString(string name, string defaultValue)
        {
            string rawValue;

            if (!_configurationOptions.TryGetValue(name.ToLowerInvariant(), out rawValue))
            {
                if (defaultValue != null)
                    return defaultValue;
                else
                    throw new ಠ_ಠ('[' + name + "] must be set");
            }

            return rawValue;
        }

        private string GetCacheDirectory(string path)
        {
            if (Path.IsPathRooted(path) == true)
                return path;

            var rawValue = _gitExecutor.GetString("rev-parse --git-dir");

            if (string.IsNullOrEmpty(rawValue))
                throw new ಠ_ಠ("Error determining .git directory");

            return Path.Combine(rawValue, path);
        }
    }
}