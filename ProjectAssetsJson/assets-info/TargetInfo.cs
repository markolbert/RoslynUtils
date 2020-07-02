﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class TargetInfo : ProjectAssetsBase, IInitializeFromNamed<ExpandoObject>
    {
        private readonly Func<ReferenceInfo> _refCreator;

        public TargetInfo(
            Func<ReferenceInfo> refCreator,
            IJ4JLogger logger
        )
            : base( logger )
        {
            _refCreator = refCreator ?? throw new NullReferenceException( nameof(refCreator) );
        }

        public CSharpFramework Target { get; set; }
        public SemanticVersion Version { get; set; } = new SemanticVersion( 0, 0, 0 );
        public List<ReferenceInfo> Packages { get; } = new List<ReferenceInfo>();

        public bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !ValidateInitializationArguments( rawName, container, context ) )
                return false;

            if( !TargetFramework.CreateTargetFramework( rawName, out var tgtFramework, Logger ) )
                return false;

            Target = tgtFramework.Framework;
            Version = tgtFramework.Version;

            Packages.Clear();

            var retVal = true;

            foreach( var kvp in container )
            {
                var newItem = _refCreator();

                if( kvp.Value is ExpandoObject childContainer )
                {
                    if( newItem.Initialize( kvp.Key, childContainer, context ) )
                        Packages.Add( newItem );
                    else
                        retVal = false;
                }
                else
                {
                    Logger.Error( $"{kvp.Key} property is not a {nameof( ExpandoObject )}" );

                    retVal = false;
                }
            }

            return retVal;
        }
    }
}