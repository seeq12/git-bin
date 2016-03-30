using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.RepresentationModel.Serialization;

namespace GitBin
{
    /// <summary>
    /// Represents a file and the chunks that it consists of. Serializes to/from yaml.
    /// </summary>
    public class GitBinDocument
    {
        public string Filename { get; private set; }
        public List<string> ChunkHashes { get; private set; }

        public GitBinDocument()
        {
            this.ChunkHashes = new List<string>();
        }

        /// <param name="filename">The name of the file that this document represents.</param>
        public GitBinDocument(string filename)
            : this()
        {
            this.Filename = filename;
        }

        /// <summary>
        /// Record a chunk that this file is made up from. This method should be called in the order that the chunks
        /// are actually present in the file. In other words, the first chunk should be recorded first and the last
        /// chunk last. All chunks added with this method can be read using the ChunkHashes property.
        /// </summary>
        /// <param name="hash">Hash of the chunk to register.</param>
        public void RecordChunk(string hash)
        {
            this.ChunkHashes.Add(hash);
        }

        /// <summary>
        /// Serialize a document to yaml.
        /// </summary>
        /// <param name="document">Document to serialize.</param>
        /// <returns>String representation of the yaml document.</returns>
        public static string ToYaml(GitBinDocument document)
        {
            var sb = new StringBuilder();
            var stringWriter = new StringWriter(sb);

            var serializer = new YamlSerializer<GitBinDocument>();
            serializer.Serialize(stringWriter, document);

            // Force line-endings to UNIX style (linefeed only) so that things are consistent across platforms
            sb.Replace("\r\n", "\n");

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize a document from a yaml file.
        /// </summary>
        /// <param name="textReader">Source of the yaml data.</param>
        /// <returns>The deserialized document.</returns>
        public static GitBinDocument FromYaml(TextReader textReader)
        {
            var yaml = textReader.ReadToEnd();

            GitBinDocument document;
            var serializer = new YamlSerializer<GitBinDocument>();

            try
            {
                document = serializer.Deserialize(new StringReader(yaml));
            }
            catch (YamlDotNet.Core.SyntaxErrorException e)
            {
                GitBinConsole.WriteLine("Syntax error in YAML file: {0}\n\n File contents:{1}\n", e.Message, yaml);
                throw;
            }

            return document;
        }
    }
}