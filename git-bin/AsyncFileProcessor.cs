using System;
using System.Collections.Generic;
using System.IO;
using GitBin.Remotes;
using System.Threading;

namespace GitBin
{
    /// <summary>
    /// A collection of methods that concurrently process files.
    /// </summary>
    public static class AsyncFileProcessor
    {
        /// <summary>
        /// Concurrently process the list of files with the given fileProcessor. Prints progress to the console. A
        /// maximum of 10 current operations will take place.
        /// </summary>
        /// <param name="filesToProcess">List of files to process.</param>
        /// <param name="fileProcessor">
        /// Processor that will act on each file. The first parameter of the processor is the file to process, the
        /// second parameter is a listener for progress information.
        /// </param>
        public static void ProcessFiles(string[] filesToProcess, Action<string, Action<int>> fileProcessor)
        {
            ProcessFiles(filesToProcess, 10, fileProcessor);
        }

        /// <summary>
        /// Concurrently process the list of files with the given fileProcessor. Prints progress to the console.
        /// </summary>
        /// <param name="filesToProcess">List of files to process.</param>
        /// <param name="maxSimultaneousProcessingOperations">Maximum number of concurrent operations.</param>
        /// <param name="fileProcessor">
        /// Processor that will act on each file. The first parameter of the processor is the file to process, the
        /// second parameter is a listener for progress information.
        /// </param>
        public static void ProcessFiles(string[] filesToProcess, int maxSimultaneousProcessingOperations,
            Action<string, Action<int>> fileProcessor)
        {
            object sync = new object();
            RemoteProgressPrinter progressPrinter = new RemoteProgressPrinter(filesToProcess.Length);

            int totalProcessing = 0;
            int processedFiles = 0;
            Exception lastException = null;

            lock (sync)
            {
                for (int indexToProcess = 0; indexToProcess < filesToProcess.Length; indexToProcess++)
                {
                    int scopedIndexToProcess = indexToProcess;
                    totalProcessing++;

                    // Add a new item to the work queue.
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        Exception exception = null;

                        try
                        {
                            fileProcessor(filesToProcess[scopedIndexToProcess], (percentComplete) =>
                                progressPrinter.OnProgressChanged(scopedIndexToProcess, percentComplete));
                        }
                        catch (Exception e)
                        {
                            // Catch any exception that might have happened while processing and store it so it can be
                            // marshalled back to the invoker thread for throwing.
                            exception = e;
                        }

                        lock (sync)
                        {
                            totalProcessing--;
                            processedFiles++;

                            // Marshalled any exeptions back to the invoker thread for throwing.
                            if (lastException == null)
                            {
                                lastException = exception;
                            }

                            Monitor.Pulse(sync);
                        }
                    });

                    // Wait for the number of outstanding operations to go lower than our max amount.
                    while (lastException == null && totalProcessing >= maxSimultaneousProcessingOperations)
                    {
                        Monitor.Wait(sync);
                    }

                    // If an exception occurred on the worker thread, throw it.
                    if (lastException != null)
                    {
                        throw lastException;
                    }
                }

                // Wait until all outstanding operations are complete.
                while (lastException == null && processedFiles < filesToProcess.Length)
                {
                    Monitor.Wait(sync);
                }

                // Tell progress printer we're done with it.
                progressPrinter.Dispose();

                // If an exception occurred on the worker thread, throw it.
                if (lastException != null)
                {
                    throw lastException;
                }
            }
        }
    }
}
