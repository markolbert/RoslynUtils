using System;
using System.IO;
using System.Reflection;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class FilePathReference : CompilationReference
    {
        public FilePathReference( string filePath )
        {
            if( String.IsNullOrEmpty( filePath ) )
                throw new NullReferenceException( $"{nameof( filePath )} is undefined or empty" );

            if( !File.Exists( filePath ) )
                throw new FileNotFoundException( $"Couldn't find {nameof( Assembly )} '{filePath}'" );

            NamePaths.Add( CompilationReferenceType.FileSystem, filePath );
        }
    }
}