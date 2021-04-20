#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.DocCompiler' is free software: you can redistribute it
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
using System.IO;
using System.Linq;
using Autofac;
using J4JSoftware.DependencyInjection;
using J4JSoftware.DocCompiler;
using J4JSoftware.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.DocCompiler
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        static CompositionRoot()
        {
            Default = new CompositionRoot();
            Default.Initialize();
        }

        public static CompositionRoot Default { get; }

        private CompositionRoot() 
            : base( "J4JSoftware", "Test.DocCompiler" )
        {
            var channelProvider = new ChannelConfigProvider( "Logging" )
                .AddChannel<DebugConfig>( "Channels:Debug" );

            ConfigurationBasedLogging( channelProvider );
        }

        public IDocScanner DocScanner => Host?.Services.GetRequiredService<IDocScanner>()!;
        public IDocDbUpdater DbUpdater => Host?.Services.GetRequiredService<IDocDbUpdater>()!;
        public Configuration Configuration => Host?.Services.GetRequiredService<Configuration>()!;

        protected override void SetupConfigurationEnvironment( IConfigurationBuilder builder )
        {
            base.SetupConfigurationEnvironment( builder );

            builder.AddJsonFile( Path.Combine( ApplicationConfigurationFolder, "appConfig.json" ) );
        }

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

            builder.Register( c => hbc.Configuration.Get<Configuration>() )
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<ScannedFileFactory>()
                .As<IScannedFileFactory>()
                .SingleInstance();

            builder.RegisterType<DocScanner>()
                .As<IDocScanner>()
                .SingleInstance();

            builder.RegisterType<DocDbUpdater>()
                .As<IDocDbUpdater>()
                .SingleInstance();

            builder.Register( c =>
                {
                    var config = c.Resolve<Configuration>();

                    if( config.Database.CreateNew && File.Exists( config.Database.Path ) )
                        File.Delete( config.Database.Path );

                    var optionsBuilder = new DbContextOptionsBuilder<DocDbContext>();
                    optionsBuilder.UseSqlite( $"Data Source={config.Database.Path}" );

                    var logger = c.Resolve<IJ4JLogger>();

                    var retVal = new DocDbContext( optionsBuilder.Options, config.Database, logger );
                    retVal.Database.EnsureCreated();

                    return retVal;
                } )
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<FullyQualifiedNodeNames>()
                .As<IFullyQualifiedNodeNames>()
                .SingleInstance();

            builder.RegisterType<NodeNames>()
                .As<INodeNames>()
                .SingleInstance();

            builder.RegisterType<INodeIdentifierTokens>()
                .As<INodeIdentifierTokens>()
                .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(DocDbContext).Assembly )
                .Where( t => typeof(IEntityProcessor).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && ( t.GetCustomAttributes( typeof(TopologicalPredecessorAttribute), false ).Any()
                                  || t.GetCustomAttributes( typeof(TopologicalRootAttribute), false ).Any() ) )
                .AsImplementedInterfaces();

            //builder.RegisterAssemblyTypes( typeof( DocDbContext ).Assembly )
            //    .Where( t => typeof( IFullyQualifiedNodeName ).IsAssignableFrom( t )
            //                 && !t.IsAbstract
            //                 && t.GetConstructors().Any() )
            //    .AsImplementedInterfaces()
            //    .SingleInstance();

            builder.RegisterType<TopologicalSortFactory>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<TypeNodeAnalyzer>()
                .As<ITypeNodeAnalyzer>()
                .SingleInstance();

            builder.RegisterType<TypeReferenceResolver>()
                .As<ITypeReferenceResolver>()
                .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(DocDbContext).Assembly )
                .Where( t => typeof(ITypeResolver).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && ( t.GetCustomAttributes( typeof(TopologicalPredecessorAttribute), false ).Any()
                                  || t.GetCustomAttributes( typeof(TopologicalRootAttribute), false ).Any() ) )
                .AsImplementedInterfaces();

            builder.RegisterType<TypeResolvers>()
                .AsSelf()
                .SingleInstance();

            //builder.RegisterType<FullyQualifiedNodeNames>()
            //    .As<IFullyQualifiedNodeNames>()
            //    .SingleInstance();

            //builder.RegisterType<NamespaceFQN>()
            //    .AsSelf()
            //    .SingleInstance();

            //builder.RegisterType<TypeParameterListFQN>()
            //    .AsSelf()
            //    .SingleInstance();

            //builder.RegisterType<NamedTypeFQN>()
            //    .AsSelf()
            //    .SingleInstance();

            //builder.RegisterType<ParameterFQN>()
            //    .AsSelf()
            //    .SingleInstance();

            //builder.RegisterType<ParameterListFQN>()
            //    .AsSelf()
            //    .SingleInstance();
        }
    }
}