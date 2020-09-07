using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectFileDependencyGroup : ProjectAssetsBase
    {
        private readonly TargetFramework _tgtFw;

#pragma warning disable 8618
        public ProjectFileDependencyGroup(
#pragma warning restore 8618
            string text,
            List<string> depGroupInfo,
            Func<IJ4JLogger> loggerFactory
        )
            : base( loggerFactory )
        {
            if( Roslyn.TargetFramework.Create( text, TargetFrameworkTextStyle.ExplicitVersion, out var tgtFW ) )
                _tgtFw = tgtFW!;
            else
                throw ProjectAssetsException.CreateAndLog(
                    $"Couldn't create a {typeof( TargetFramework )} from property '{text}'",
                    this.GetType(),
                    Logger );

            CreateDependencies( depGroupInfo );
        }

        private void CreateDependencies( List<string> depGroupInfo )
        {
            foreach( var depInfo in depGroupInfo )
            {
                var parts = depInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 3)
                    throw ProjectAssetsException.CreateAndLog(
                        "Couldn't parse assembly constraint",
                        this.GetType(),
                        Logger);

                var version = GetSemanticVersion( parts[ 2 ] );

                var constraint = parts[1] switch
                {
                    "==" => VersionConstraint.EqualTo,
                    "<=" => VersionConstraint.Maximum,
                    ">=" => VersionConstraint.Minimum,
                    ">" => VersionConstraint.GreaterThan,
                    "<" => VersionConstraint.LessThan,
                    _ => VersionConstraint.Undefined
                };

                Dependencies.Add( new ProjectFileDependencyGroupDependency( parts[ 0 ], version, constraint, LoggerFactory ) );
            }
        }

        public CSharpFramework TargetFramework => _tgtFw.Framework;
        public SemanticVersion TargetVersion => _tgtFw.Version;
        public List<ProjectFileDependencyGroupDependency> Dependencies { get; } = new List<ProjectFileDependencyGroupDependency>();
    }
}
