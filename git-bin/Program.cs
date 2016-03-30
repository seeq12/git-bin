using System;
using System.Threading;
using Objector;

namespace GitBin
{
    class Program
    {
        static int Main(string[] args)
        {
            if (Environment.GetEnvironmentVariable("GIT_BIN_ATTACH_DEBUGGER") != null)
            {
                GitBinConsole.WriteLine("Waiting for debugger to attach to this process (PID: {0})", System.Diagnostics.Process.GetCurrentProcess().Id);
                while (System.Diagnostics.Debugger.IsAttached == false)
                {
                    Thread.Sleep(500);
                }
                System.Diagnostics.Debugger.Break();
            }

            try
            {
                // Build the list of available commands and execute the one requested in the user-provided args.
                var builder = new Builder();
                ApplicationRegistrations.Register(builder);
                var container = builder.Create();

                var commandFactory = container.Resolve<ICommandFactory>();

                var command = commandFactory.GetCommand(args);
                command.Execute();
            }
            catch (ಠ_ಠ lod)
            {
                GitBinConsole.WriteLine(lod.Message);
                return 1;
            }
            catch (Exception e)
            {
                GitBinConsole.WriteLine("Uncaught exception, please report this bug! " + e);
                return 2;
            }

            return 0;
        }
    }
}
