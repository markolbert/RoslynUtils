using System.Reflection;
using System.Runtime.Loader;

namespace J4JSoftware.Roslyn
{
    public class CompilationLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public CompilationLoadContext( string mainAssemblyToLoadPath )
        {
            _resolver = new AssemblyDependencyResolver( mainAssemblyToLoadPath );
        }

        protected override Assembly? Load( AssemblyName assemblyName )
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath( assemblyName );

            if( assemblyPath != null )
            {
                return LoadFromAssemblyPath( assemblyPath );
            }

            return null;
        }
    }
}