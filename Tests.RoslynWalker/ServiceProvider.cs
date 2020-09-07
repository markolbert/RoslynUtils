using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
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

            builder.RegisterType<SharpObjectTypeMapper>()
                .As<ISharpObjectTypeMapper>()
                .SingleInstance();

            builder.RegisterType<SymbolNamer>()
                .As<ISymbolNamer>()
                .SingleInstance();

            builder.RegisterType<RoslynDbContextFactoryConfiguration>()
                .AsImplementedInterfaces();

            builder.RegisterType<RoslynDbContext>()
                .AsSelf();

            builder.RegisterAssemblyTypes( typeof(AssemblyWalker).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(ISyntaxWalker).IsAssignableFrom( t )
                             && t.GetConstructors().Length > 0 )
                .AsImplementedInterfaces();

            builder.RegisterType<SyntaxWalkers>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(ISymbolSink).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<DefaultSymbolSink>()
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<IAssemblySymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<AssemblyProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<InScopeAssemblyProcessor>()
                .As<IInScopeAssemblyProcessor>();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<INamespaceSymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<NamespaceProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<ITypeSymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<TypeProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();
            
            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAtomicProcessor<IMethodSymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<MethodProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();

            //builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
            //    .Where(t => !t.IsAbstract
            //                && typeof(IAtomicProcessor<IPropertySymbol>).IsAssignableFrom(t)
            //                && t.GetConstructors().Length > 0)
            //    .AsImplementedInterfaces();

            //builder.RegisterType<PropertyProcessors>()
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(RoslynDbContext).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(IEntityFactory).IsAssignableFrom( t )
                             && t.GetConstructors().Length > 0 )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<EntityFactories>()
                .As<IEntityFactories>()
                .SingleInstance();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}