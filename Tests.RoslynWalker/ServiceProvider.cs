#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Linq;
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
        static ServiceProvider()
        {
            var builder = new ContainerBuilder();

            var loggerConfig = new J4JLoggerConfiguration { EventElements = EventElements.All };

            loggerConfig.Channels.Add( new ConsoleConfig { MinimumLevel = LogEventLevel.Information } );
            loggerConfig.Channels.Add( new DebugConfig { MinimumLevel = LogEventLevel.Information } );

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

            builder.RegisterType<SyntaxWalkerNG>()
                .AsImplementedInterfaces();

            builder.RegisterType<SyntaxWalkers>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(RoslynDbContext).Assembly )
                .Where( t => !t.IsAbstract
                             && typeof(ISymbolSink).IsAssignableFrom( t )
                             && t.GetConstructors().Length > 0 )
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
            RegisterSymbolProcessor<IEventSymbol, EventProcessors>( builder );
            RegisterSymbolProcessor<ISymbol, AttributeProcessors>( builder );

            builder.RegisterType<SymbolCollector>()
                .As<ISyntaxNodeSink>()
                .SingleInstance();

            builder.RegisterType<WalkerContext>()
                .AsSelf();

            Instance = new AutofacServiceProvider( builder.Build() );
        }

        public static IServiceProvider Instance { get; }

        private static IRegistrationBuilder<TProcessors, ConcreteReflectionActivatorData, SingleRegistrationStyle>
            RegisterSymbolProcessor<TSymbol, TProcessors>( ContainerBuilder builder )
            where TSymbol : ISymbol
            where TProcessors : RoslynDbProcessors<TSymbol>
        {
            builder.RegisterAssemblyTypes( typeof(RoslynDbContext).Assembly )
                .Where( t =>
                {
                    if( t.IsAbstract )
                        return false;

                    if( !typeof(IAction).IsAssignableFrom( t ) )
                        return false;

                    return t.GetConstructors().Length > 0;
                } )
                .AsImplementedInterfaces();

            return builder.RegisterType<TProcessors>()
                .AsImplementedInterfaces()
                .SingleInstance();
        }
    }
}