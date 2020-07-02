using System;

namespace J4JSoftware.Roslyn
{
    public class NamedPathReference : CompilationReference
    {
        public NamedPathReference( string assemblyName, string assemblyPath )
        {
            if( String.IsNullOrEmpty(assemblyName))
                throw new NullReferenceException( $"{nameof( assemblyName )} is undefined or empty" );

            if( String.IsNullOrEmpty( assemblyPath ) )
                throw new NullReferenceException( $"{nameof( assemblyPath )} is undefined or empty" );

            NamePaths.Add( CompilationReferenceType.AssemblyName, assemblyName );
            NamePaths.Add( CompilationReferenceType.FileSystem, assemblyPath );
        }
    }
}