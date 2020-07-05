﻿using System;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class FrameworkBase : ConfigurationBase
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
            if( Roslyn.TargetFramework.Create( text, TargetFrameworkTextStyle.Simple, out var tgtFW ) )
                _tgtFW = tgtFW!;
            else LogAndThrow( $"Couldn't a {typeof(TargetFramework)}", text );
        }

        public CSharpFramework TargetFramework => _tgtFW.Framework;
        public SemanticVersion TargetVersion => _tgtFW.Version;
    }
}