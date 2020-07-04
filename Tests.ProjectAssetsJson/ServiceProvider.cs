using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Serilog.Events;

namespace Tests.ProjectAssetsJson
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

            builder.RegisterType<PackageLibrary>()
                .AsSelf();

            builder.RegisterType<ProjectLibrary>()
                .AsSelf();

            builder.RegisterType<TargetInfo>()
                .AsSelf();

            builder.RegisterType<ProjectInfo>()
                .AsSelf();

            builder.RegisterType<ReferenceInfo>()
                .AsSelf();

            builder.RegisterType<ProjectReference>()
                .AsSelf();

            builder.RegisterType<DependencyInfo>()
                .AsSelf();

            builder.RegisterType<RestrictedDependencyInfo>()
                .AsSelf();

            builder.RegisterType<RestoreInfo>()
                .AsSelf();

            builder.RegisterType<FrameworkReferences>()
                .AsSelf();

            builder.RegisterType<WarningProperty>()
                .AsSelf();

            builder.RegisterType<ProjectFileDependencyGroup>()
                .AsSelf();

            builder.RegisterType<JsonProjectAssetsConverter>()
                .AsSelf();

            builder.RegisterType<ProjectAssets>()
                .AsSelf();

            Instance = new AutofacServiceProvider( builder.Build() );
        }
    }
}