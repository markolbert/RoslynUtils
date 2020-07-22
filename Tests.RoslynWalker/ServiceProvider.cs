using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.sinks;
using J4JSoftware.Roslyn.walkers;
using Serilog.Events;

namespace Tests.RoslynWalker
{
    public class ServiceProvider
    {
        public static IServiceProvider Instance { get; }

        static ServiceProvider()
        {
            var builder = new ContainerBuilder();

            builder.Register( c =>
                {
                    var retVal = new J4JLoggerConfiguration { EventElements = EventElements.All };

                    retVal.Channels.Add( new ConsoleChannel() { MinimumLevel = LogEventLevel.Verbose } );
                    retVal.Channels.Add( new DebugChannel() { MinimumLevel = LogEventLevel.Verbose } );

                    return retVal;
                } )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterLogger();

            builder.RegisterType<TypedListCreator>()
                .AsImplementedInterfaces();

            builder.RegisterType<JsonProjectAssetsConverter>()
                .AsSelf();

            builder.RegisterType<ProjectModels>()
                .AsSelf();

            builder.RegisterType<SyntaxWalkers>()
                .AsSelf();

            builder.RegisterType<AssemblyWalker>()
                .AsImplementedInterfaces();

            builder.RegisterType<DefaultSymbolSink>()
                .AsImplementedInterfaces();

            builder.RegisterType<SymbolName>()
                .AsImplementedInterfaces();

            builder.RegisterType<AssemblySink>()
                .AsImplementedInterfaces();

            builder.RegisterType<InScopeAssemblyProcessor>()
                .As<IInScopeAssemblyProcessor>();

            builder.RegisterType<RoslynDbContext>()
                //.OnActivated( x => x.Instance.Database.Migrate() )
                .AsSelf();

            builder.RegisterType<RoslynDbContextFactoryConfiguration>()
                .AsImplementedInterfaces();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}