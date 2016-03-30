using System;

namespace GitBin
{
    /// <summary>
    /// Helper methods for writing to the console.
    /// </summary>
    public static class GitBinConsole
    {
        private const string Prefix = "[git-bin] ";

        /// <summary>
        /// Write a message to the console. The message will be prefixed with an identifier indicating it originated
        /// from this application. No newline will be inserted after the message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        public static void Write(string message)
        {
            Console.Error.Write(Prefix + message);
        }

        /// <summary>
        /// Write a message to the console. The message will be prefixed with an identifier indicating it originated
        /// from this application. No newline will be inserted after the message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        /// <param name="args">Arguments to insert into the given message.</param>
        public static void Write(string message, params object[] args)
        {
            Console.Error.Write(Prefix + message, args);
        }

        /// <summary>
        /// Write a message to the console. The message will be prefixed with an identifier indicating it originated
        /// from this application. A newline will be inserted after the message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        /// <param name="args">Arguments to insert into the given message.</param>
        public static void WriteLine(string message, params object[] args)
        {
            Console.Error.WriteLine(Prefix + message, args);
        }

        /// <summary>
        /// Write a message to the console. No newline will be inserted after the message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        public static void WriteNoPrefix(string message)
        {
            Console.Error.Write(message);
        }

        /// <summary>
        /// Write a message to the console. No newline will be inserted after the message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        /// <param name="args">Arguments to insert into the given message.</param>
        public static void WriteNoPrefix(string message, params object[] args)
        {
            Console.Error.Write(message, args);
        }

        /// <summary>
        /// Write a message to the console. A newline will be inserted after the message.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        /// <param name="args">Arguments to insert into the given message.</param>
        public static void WriteLineNoPrefix(string message, params object[] args)
        {
            Console.Error.WriteLine(message, args);
        }

        /// <summary>
        /// Write a newline to the console.
        /// </summary>
        public static void WriteLine()
        {
            Console.Error.WriteLine();
        }

        /// <summary>
        /// Ensures all data is written to the console.
        /// </summary>
        public static void Flush()
        {
            Console.Error.Flush();
        }
    }
}