using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Roslyn.Sinks;
using J4JSoftware.Roslyn.walkers;
using Microsoft.CodeAnalysis;
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

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(ISymbolSink).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<IAssemblySymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<INamespaceSymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<ITypeSymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<IMethodSymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<IPropertySymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<DocObjectTypeMapper>()
                .As<IDocObjectTypeMapper>()
                .SingleInstance();

            builder.RegisterType<SyntaxWalkers>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<AssemblyProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<NamespaceProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<TypeProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<MethodProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<PropertyProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<DefaultSymbolSink>()
                .AsImplementedInterfaces();

            builder.RegisterType<SymbolNamer>()
                .As<ISymbolNamer>()
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