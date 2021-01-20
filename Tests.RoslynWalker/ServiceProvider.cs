using System;
using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
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

            var loggerConfig = new J4JLoggerConfiguration { EventElements = EventElements.All };

            loggerConfig.Channels.Add( new ConsoleConfig() { MinimumLevel = LogEventLevel.Information } );
            loggerConfig.Channels.Add( new DebugConfig() { MinimumLevel = LogEventLevel.Information } );

            builder.RegisterJ4JLogging( loggerConfig );

            builder.RegisterType<DocumentationWorkspace>()
                .AsSelf();

            builder.RegisterType<SymbolFullName>()
                .As<ISymbolFullName>()
                .SingleInstance();

            builder.RegisterType<ActionsContext>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<RoslynDbContextFactoryConfiguration>()
                .AsImplementedInterfaces();

            builder.RegisterType<RoslynDbContext>()
                .AsSelf();

            builder.RegisterType<RoslynDataLayer>()
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(AssemblyWalker).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(ISyntaxWalker).IsAssignableFrom( t )
                             && t.GetConstructors().Length > 0 )
                .AsImplementedInterfaces();

            builder.RegisterType<SyntaxWalkers>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(ISymbolSink).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            builder.RegisterType<DefaultSymbolSink>()
                .AsImplementedInterfaces();

            builder.RegisterGeneric( typeof(UniqueSymbols<>) )
                .AsSelf();

            RegisterSymbolProcessor<IAssemblySymbol, AssemblyProcessors>( builder );
            RegisterSymbolProcessor<INamespaceSymbol, NamespaceProcessors>( builder );
            RegisterSymbolProcessor<ITypeSymbol, TypeProcessors>( builder );
            RegisterSymbolProcessor<IMethodSymbol, MethodProcessors>( builder );
            RegisterSymbolProcessor<IPropertySymbol, PropertyProcessors>( builder );
            RegisterSymbolProcessor<IFieldSymbol, FieldProcessors>( builder );
            RegisterSymbolProcessor<IEventSymbol, EventProcessors>(builder);
            RegisterSymbolProcessor<ISymbol, AttributeProcessors>(builder);

            builder.RegisterType<SingleWalker>()
                .As<ISingleWalker>()
                .SingleInstance();

            builder.RegisterType<SymbolCollector>()
                .As<ISyntaxNodeSink>()
                .SingleInstance();

            Instance = new AutofacServiceProvider( builder.Build() );
        }

        private static IRegistrationBuilder<TProcessors, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterSymbolProcessor<TSymbol, TProcessors>( ContainerBuilder builder )
            where TSymbol : ISymbol
            where TProcessors : RoslynDbProcessors<TSymbol>
        {
            builder.RegisterAssemblyTypes(typeof(RoslynDbContext).Assembly)
                .Where(t => !t.IsAbstract
                            && typeof(IAction<TSymbol>).IsAssignableFrom(t)
                            && t.GetConstructors().Length > 0)
                .AsImplementedInterfaces();

            return builder.RegisterType<TProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}