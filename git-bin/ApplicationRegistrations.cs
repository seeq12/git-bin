using GitBin.Commands;
using GitBin.Remotes;
using Objector;

namespace GitBin
{
    /// <summary>
    /// Convenience methods for registering commands that the application supports.
    /// </summary>
    public static class ApplicationRegistrations
    {
        /// <summary>
        /// Registers all known commands for this application so they can be invoked by the user. Invoke this method on
        /// application start up. New commands should be added to this method.
        /// </summary>
        /// <param name="builder">The builder to register all commands to.</param>
        public static void Register(IBuilder builder)
        {
            builder.RegisterAssembly(typeof(CommandFactory).Assembly)
                .InNamespaceOf<CommandFactory>()
                .AsImplementedInterfaces();

            builder.RegisterAssembly(typeof(IRemote).Assembly)
                .InNamespaceOf<IRemote>()
                .AsImplementedInterfaces();

            builder.RegisterAssembly(typeof(CleanCommand).Assembly)
                .InNamespaceOf<CleanCommand>()
                .AsSelf();

            builder.RegisterFactory<ShowUsageCommand>();
            builder.RegisterFactory<VersionCommand>();
            builder.RegisterFactory<string, PrintCommand>();
            builder.RegisterFactory<string[], CleanCommand>();
            builder.RegisterFactory<string[], ClearCommand>();
            builder.RegisterFactory<string[], SmudgeCommand>();
            builder.RegisterFactory<string[], PushCommand>();
            builder.RegisterFactory<string[], StatusCommand>();
        }
    }
}