using System;
using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class FrameworkLibraryReference : ProjectAssetsBase
    {
        public FrameworkLibraryReference( 
            string text,
            ExpandoObject fwlrInfo,
            Func<IJ4JLogger> loggerFactory
            ) 
            : base( loggerFactory )
        {
            LibraryName = text;
            PrivateAssets = GetProperty<string>( fwlrInfo, "privateAssets" );
        }

        public string LibraryName { get; }
        public string PrivateAssets { get; }
    }
}