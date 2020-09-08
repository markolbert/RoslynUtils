using System;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class NamedReference : CompilationReference
    {
        public NamedReference( string assemblyName )
        {
            if( String.IsNullOrEmpty(assemblyName))
                throw new NullReferenceException( $"{nameof( assemblyName )} is undefined or empty" );

            NamePaths.Add( CompilationReferenceType.AssemblyName, assemblyName );
        }
    }
}