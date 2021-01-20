using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.ProjectAssets;
using Serilog.Events;

namespace Tests.ProjectAssetsJson
{
    public class ServiceProvider
    {
        public static IServiceProvider Instance { get; }

        static ServiceProvider()
        {
            var builder = new ContainerBuilder();

            var loggerConfig = new J4JLoggerConfiguration { EventElements = EventElements.All };

            loggerConfig.Channels.Add( new ConsoleConfig() { MinimumLevel = LogEventLevel.Verbose } );
            loggerConfig.Channels.Add( new DebugConfig() { MinimumLevel = LogEventLevel.Verbose } );

            builder.RegisterJ4JLogging( loggerConfig );

            builder.RegisterType<TypedListCreator>()
                .AsImplementedInterfaces();

            builder.RegisterType<JsonProjectAssetsConverter>()
                .AsSelf();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}