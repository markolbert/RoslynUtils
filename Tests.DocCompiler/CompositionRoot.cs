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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Tests.DocCompiler
{
    public class CompositionRoot : J4JCompositionRoot<J4JLoggerConfiguration>
    {
        public static CompositionRoot? _default;

        public static CompositionRoot Default
        {
            get
            {
                if( _default != null ) 
                    return _default;

                _default = new CompositionRoot();
                _default.Initialize();

                return _default;
            }
        }

        private CompositionRoot() 
            : base( "J4JSoftware", "Test.DocCompiler" )
        {
            var loggerConfig = new J4JLoggerConfiguration
            {
                EventElements = EventElements.All,
                MultiLineEvents = true,
                SourceRootPath = "c:/Programming/RoslynUtils/Tests.DocCompiler",
            };

            loggerConfig.Channels.Add(new DebugConfig());

            StaticConfiguredLogging( loggerConfig );
        }

        public IDocScanner DocScanner => Host?.Services.GetRequiredService<IDocScanner>()!;
        public IDocDbUpdater DbUpdater => Host?.Services.GetRequiredService<IDocDbUpdater>()!;

        protected override void SetupDependencyInjection( HostBuilderContext hbc, ContainerBuilder builder )
        {
            base.SetupDependencyInjection( hbc, builder );

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
                    //var dbPath = Path.Combine( Environment.CurrentDirectory, "DocCompiler.db" );

                    //if( File.Exists(dbPath))
                    //    File.Delete(dbPath);

                    var optionsBuilder = new DbContextOptionsBuilder<DocDbContext>();
                    optionsBuilder.UseSqlite( "Data Source=DocCompiler.db" );

                    var logger = c.Resolve<IJ4JLogger>();

                    var retVal = new DocDbContext( optionsBuilder.Options, logger );
                    retVal.Database.EnsureCreated();

                    return retVal;
                } )
                .AsSelf()
                .SingleInstance();

            builder.RegisterAssemblyTypes( typeof(DocDbContext).Assembly )
                .Where( t => typeof(IEntityProcessor).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && t.GetCustomAttributes( typeof(TopologicalPredecessorAttribute), false ).Any()
                             || t.GetCustomAttributes( typeof(TopologicalRootAttribute), false ).Any() )
                .AsImplementedInterfaces();

            builder.RegisterAssemblyTypes( typeof( DocDbContext ).Assembly )
                .Where( t => typeof( IFullyQualifiedName ).IsAssignableFrom( t )
                             && !t.IsAbstract
                             && t.GetConstructors().Any() )
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterType<FullyQualifiedNames>()
                .As<IFullyQualifiedNames>()
                .SingleInstance();

            builder.RegisterType<NamespaceFQN>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<TypeParameterListFQN>()
                .AsSelf()
                .SingleInstance();
        }
    }
}