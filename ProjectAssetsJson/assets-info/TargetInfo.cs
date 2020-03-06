using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class TargetInfo : ILoadFromNamed<ExpandoObject>
    {
        private readonly Func<ReferenceInfo> _refCreator;
        private readonly IJ4JLogger<TargetInfo> _logger;

        public TargetInfo(
            Func<ReferenceInfo> refCreator,
            IJ4JLogger<TargetInfo> logger
        )
        {
            _refCreator = refCreator ?? throw new NullReferenceException( nameof( refCreator ) );
            _logger = logger ?? throw new NullReferenceException( nameof( logger ) );
        }

        public CSharpFrameworks Target { get; set; }
        public SemanticVersion Version { get; set; }
        public List<ReferenceInfo> Packages { get; set; }

        public bool Load( string rawName, ExpandoObject container )
        {
            if( container == null )
            {
                _logger.Error( $"Undefined {nameof( container )}" );

                return false;
            }

            if( String.IsNullOrEmpty( rawName ) )
            {
                _logger.Error( $"Undefined or empty {nameof( rawName )}" );

                return false;
            }

            if( !TargetFramework.CreateTargetFramework( rawName, out var tgtFramework, _logger ) )
                return false;

            Target = tgtFramework.Framework;
            Version = tgtFramework.Version;

            Packages = new List<ReferenceInfo>();

            var retVal = true;

            foreach( var kvp in container )
            {
                var newItem = _refCreator();

                if( kvp.Value is ExpandoObject childContainer )
                {
                    if( newItem.Load( kvp.Key, childContainer ) )
                        Packages.Add( newItem );
                    else
                        retVal = false;
                }
                else
                {
                    _logger.Error( $"{kvp.Key} property is not a {nameof( ExpandoObject )}" );

                    retVal = false;
                }
            }

            return retVal;
        }
    }
}