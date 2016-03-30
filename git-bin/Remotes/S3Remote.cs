using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Security.Cryptography;
using System.Data;
using Amazon.Runtime;

namespace GitBin.Remotes
{
    /// <summary>
    /// Enables files to be stored and retrieved on Amazon's S3 service.
    /// </summary>
    public class S3Remote : IRemote
    {
        private const string S3KeyConfigName = "s3key";
        private const string S3SecretKeyConfigName = "s3secretKey";
        private const string S3BucketConfigName = "s3bucket";

        private const int RequestTimeoutInMinutes = 10;
        private const string InvalidAccessKeyErrorCode = "InvalidAccessKeyId";
        private const string InvalidSecurityErrorCode = "InvalidSecurity";

        private readonly string _bucketName;
        private readonly string _key;
        private readonly string _secretKey;
        private readonly AmazonS3Config _s3config;

        private IAmazonS3 _client;

        public S3Remote(IConfigurationProvider configurationProvider)
        {
            _bucketName = configurationProvider.S3Bucket;
            _key = configurationProvider.S3Key;
            _secretKey = configurationProvider.S3SecretKey;

            AmazonS3Config s3config = new AmazonS3Config();
            s3config.UseHttp = !String.Equals(configurationProvider.Protocol, "HTTPS",
                StringComparison.OrdinalIgnoreCase);
            s3config.RegionEndpoint = RegionEndpoint.GetBySystemName(configurationProvider.S3SystemName);

            s3config.ReadWriteTimeout = TimeSpan.FromMinutes(RequestTimeoutInMinutes);
            s3config.Timeout = TimeSpan.FromMinutes(RequestTimeoutInMinutes);

            _s3config = s3config;
        }

        public GitBinFileInfo[] ListFiles()
        {
            var remoteFiles = new List<GitBinFileInfo>();
            var client = GetClient();

            var listRequest = new ListObjectsRequest();
            listRequest.BucketName = _bucketName;
            listRequest.MaxKeys = 250000; // Arbitrarily large to avoid making multiple round-trips.

            do
            {
                ListObjectsResponse response = client.ListObjects(listRequest);

                // Process response.
                if (response.S3Objects.Any())
                {
                    try
                    {
                        var keys = response.S3Objects.Select(o => new GitBinFileInfo(o.Key, o.Size));

                        remoteFiles.AddRange(keys);
                    }
                    catch (AmazonS3Exception e)
                    {
                        throw new ಠ_ಠ(GetMessageFromException(e));
                    }
                }

                // If response is truncated, set the marker to get the next set of keys.
                if (response.IsTruncated)
                {
                    listRequest.Marker = response.NextMarker;
                }
                else
                {
                    listRequest = null;
                }
            } while (listRequest != null);

            return remoteFiles.ToArray();
        }

        public void UploadFile(string sourceFilePath, string destinationFileName, Action<int> progressListener)
        {
            var client = GetClient();

            var putRequest = new PutObjectRequest();
            putRequest.BucketName = _bucketName;
            putRequest.FilePath = sourceFilePath;
            putRequest.Key = destinationFileName;
            putRequest.Timeout = TimeSpan.FromMinutes(RequestTimeoutInMinutes);
            putRequest.MD5Digest = GetMd5Hash(sourceFilePath);
            putRequest.StreamTransferProgress += (s, args) => progressListener(args.PercentDone);

            try
            {
                client.PutObject(putRequest);
            }
            catch (AmazonS3Exception e)
            {
                if (e.ErrorCode != null && e.ErrorCode.Equals("InvalidDigest"))
                {
                    throw new ಠ_ಠ("MD5 hash is invalid. Data was malformed in transit");
                }
                else
                {
                    throw new ಠ_ಠ(GetMessageFromException(e));
                }
            }
        }

        public byte[] DownloadFile(string fileName, Action<int> progressListener)
        {
            var client = GetClient();

            var getRequest = new GetObjectRequest();
            getRequest.BucketName = _bucketName;
            getRequest.Key = fileName;

            try
            {
                using (var getResponse = client.GetObject(getRequest))
                {
                    var fileContent = new byte[getResponse.ContentLength];

                    if (getResponse.ContentLength > 0)
                    {
                        var numberOfBytesRead = 0;
                        var totalBytesRead = 0;

                        do
                        {
                            numberOfBytesRead = getResponse.ResponseStream.Read(fileContent, totalBytesRead,
                                fileContent.Length - totalBytesRead);
                            if (numberOfBytesRead == 0)
                            {
                                throw new ಠ_ಠ(String.Format(
                                    "S3 download stream ended before complete file was read: {0}", fileName));
                            }

                            totalBytesRead += numberOfBytesRead;
                            progressListener.Invoke((totalBytesRead * 100) / fileContent.Length);
                        } while (totalBytesRead < fileContent.Length);
                    }
                    else
                    {
                        progressListener.Invoke(100);
                    }

                    return fileContent;
                }
            }
            catch (AmazonS3Exception e)
            {
                if (e.ErrorCode != null && e.ErrorCode.Equals("NoSuchKey"))
                {
                    throw new ಠ_ಠ(String.Format("File not found on S3: {0}", fileName));
                }
                else
                {
                    throw new ಠ_ಠ(GetMessageFromException(e));
                }
            }
        }

        private IAmazonS3 GetClient()
        {
            if (_client == null)
            {
                _client = AWSClientFactory.CreateAmazonS3Client(_key, _secretKey, _s3config);
            }

            return _client;
        }

        private string GetMessageFromException(AmazonS3Exception e)
        {
            if (!String.IsNullOrEmpty(e.ErrorCode) &&
                (e.ErrorCode == InvalidAccessKeyErrorCode ||
                 e.ErrorCode == InvalidSecurityErrorCode))
                return "S3 error: check your access key and secret access key";

            return String.Format("S3 error: code [{0}], message [{1}]", e.ErrorCode, e.Message);
        }

        protected string GetMd5Hash(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
        }
    }
}