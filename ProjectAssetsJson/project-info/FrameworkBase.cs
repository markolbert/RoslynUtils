using System;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class FrameworkBase : ProjectAssetsBase
    {
        private readonly TargetFramework _tgtFW;

#pragma warning disable 8618
        protected FrameworkBase(
#pragma warning restore 8618
            string text,
            Func<IJ4JLogger> loggerFactory
            ) 
            : base( loggerFactory )
        {
            _tgtFW = GetTargetFramework( text, TargetFrameworkTextStyle.Simple );
        }

        public CSharpFramework TargetFramework => _tgtFW.Framework;
        public SemanticVersion TargetVersion => _tgtFW.Version;
    }
}