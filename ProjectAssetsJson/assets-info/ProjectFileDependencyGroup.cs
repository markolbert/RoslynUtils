using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis.Diagnostics;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class ProjectFileDependencyGroup : ConfigurationBase
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
            else LogAndThrow( $"Couldn't create a {typeof(TargetFramework)}", text );

            CreateDependencies( depGroupInfo );
        }

        private void CreateDependencies( List<string> depGroupInfo )
        {
            foreach( var depInfo in depGroupInfo )
            {
                var parts = depInfo.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 3)
                    LogAndThrow("Couldn't parse assembly constraint", depInfo);

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

                Dependencies.Add( new RestrictedDependencyInfo( parts[ 0 ], version, constraint, LoggerFactory ) );
            }
        }

        public CSharpFramework TargetFramework => _tgtFw.Framework;
        public SemanticVersion TargetVersion => _tgtFw.Version;
        public List<RestrictedDependencyInfo> Dependencies { get; } = new List<RestrictedDependencyInfo>();
    }
}
