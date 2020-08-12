using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.Sinks;
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

            builder.RegisterType<DocumentationWorkspace>()
                .AsSelf();

            builder.RegisterType<SyntaxWalkers>()
                .AsSelf();

            builder.RegisterAssemblyTypes( typeof(AssemblyWalker).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(ISyntaxWalker).IsAssignableFrom( t )
                             && t.GetConstructors().Length > 0 )
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbSink<,>).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(ISymbolSink).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<TypeProcessorContext>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<SyntaxWalkers>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<TypeDefinitionProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<DefaultSymbolSink>()
                .AsImplementedInterfaces();

            builder.RegisterType<SymbolInfo.Factory>()
                .As<ISymbolInfo>()
                .SingleInstance();

            builder.RegisterType<InScopeAssemblyProcessor>()
                .As<IInScopeAssemblyProcessor>();

            builder.RegisterType<RoslynDbContext>()
                .AsSelf();

            builder.RegisterType<RoslynDbContextFactoryConfiguration>()
                .AsImplementedInterfaces();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}