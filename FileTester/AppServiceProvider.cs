using Autofac;
using Autofac.Extensions.DependencyInjection;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn.Testing
{
    internal static class AppServiceProvider
    {
        private static AutofacServiceProvider _svcProvider;

        public static AutofacServiceProvider Instance => _svcProvider ??= ConfigureContainer();

        private static AutofacServiceProvider ConfigureContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<J4JLoggerConfiguration>()
                .As<IJ4JLoggerConfiguration>()
                .SingleInstance();

            builder.RegisterType<ProjectAssets>()
                .AsSelf();

            builder.RegisterType<TargetInfo>()
                .AsSelf();

            builder.RegisterType<ReferenceInfo>()
                .AsSelf();

            builder.RegisterType<TargetDependency>()
                .AsSelf();

            builder.RegisterType<ProjectFileDependencyGroupDependency>()
                .AsSelf();

            builder.RegisterType<LibraryInfo>()
                .AsSelf();

            builder.RegisterType<ProjectFileDependencyGroup>()
                .AsSelf();

            builder.RegisterType<ProjectInfo>()
                .AsSelf();

            builder.RegisterType<RestoreInfo>()
                .AsSelf();

            builder.RegisterType<ProjectFramework>()
                .AsSelf();

            builder.RegisterType<WarningProperty>()
                .AsSelf();

            //builder.RegisterType<ProjectReference>()
            //    .AsSelf();

            builder.RegisterType<JsonProjectAssetsConverter>()
                .AsSelf();

            builder.RegisterType<TypedListCreator>()
                .As<ITypedListCreator>();

            return new AutofacServiceProvider( builder.Build() );
        }
    }
}
