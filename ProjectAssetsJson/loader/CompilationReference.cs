using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class CompilationReference
    {
        private readonly Dictionary<CompilationReferenceType, Exception> _exceptions =
            new Dictionary<CompilationReferenceType, Exception>();

        protected CompilationReference()
        {
        }

        protected Dictionary<CompilationReferenceType, string> NamePaths { get; } =
            new Dictionary<CompilationReferenceType, string>();

        public bool FromSourceCode { get; set; }
        public bool IsVirtual { get; set; }

        public string? AssemblyName => NamePaths.ContainsKey( CompilationReferenceType.AssemblyName )
            ? NamePaths[ CompilationReferenceType.AssemblyName ]
            : null;

        public Microsoft.CodeAnalysis.MetadataReference? Reference { get; protected set; }
        public List<string> ReferencedAssemblies { get; } = new List<string>();
        public CompilationReferenceType LoadFailures { get; protected set; }
        public bool Succeeded => LoadFailures == CompilationReferenceType.None;

        public ReadOnlyDictionary<CompilationReferenceType, Exception> Exceptions =>
            new ReadOnlyDictionary<CompilationReferenceType, Exception>( _exceptions );

        public bool Load( AssemblyLoadContext context )
        {
            if( NamePaths.Count == 0 || context == null )
            {
                LoadFailures = CompilationReferenceType.All;
                return false;
            }

            LoadFailures = CompilationReferenceType.None;
            _exceptions.Clear();
            Reference = null;

            // if loading by name didn't work, next try loading from the file path, if one exists
            if( NamePaths.ContainsKey( CompilationReferenceType.FileSystem )
                && LoadFromFilePath( context ) )
                return true;

            // start by trying to load the assembly by name since the framework is much
            // cleverer than me at finding assemblies...
            if( NamePaths.ContainsKey( CompilationReferenceType.AssemblyName )
                && LoadFromAssemblyName( context ) )
                return true;

            return false;
        }

        private bool LoadFromAssemblyName( AssemblyLoadContext context )
        {
            try
            {
                var aName = NamePaths[ CompilationReferenceType.AssemblyName ];

                var assembly = context.LoadFromAssemblyName( new AssemblyName( aName ) );
                if( assembly == null )
                    return false;

                if( !NamePaths.ContainsKey( CompilationReferenceType.FileSystem ) )
                    NamePaths.Add( CompilationReferenceType.FileSystem, assembly.Location );

                Reference = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile( assembly.Location );

                ReferencedAssemblies.Clear();
                ReferencedAssemblies.AddRange( assembly.GetReferencedAssemblies()
                    .Select( ra => ra.Name! ) );
            }
            catch( Exception e )
            {
                LoadFailures |= CompilationReferenceType.AssemblyName;
                _exceptions.Add( CompilationReferenceType.AssemblyName, e );

                return false;
            }

            return true;
        }

        private bool LoadFromFilePath( AssemblyLoadContext context )
        {
            try
            {
                var path = NamePaths[ CompilationReferenceType.FileSystem ];
                var assembly = context.LoadFromAssemblyPath( path );

                if( !NamePaths.ContainsKey( CompilationReferenceType.AssemblyName ) )
                    NamePaths.Add( CompilationReferenceType.AssemblyName, assembly.FullName! );

                Reference = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile( assembly.Location );

                ReferencedAssemblies.Clear();
                ReferencedAssemblies.AddRange( assembly.GetReferencedAssemblies()
                    .Select( ra => ra.Name! ) );
            }
            catch( Exception e )
            {
                LoadFailures |= CompilationReferenceType.FileSystem;
                _exceptions.Add( CompilationReferenceType.FileSystem, e );

                return false;
            }

            return true;
        }

        private bool CheckAssembly( Assembly toCheck )
        {
            if( toCheck == null )
                return false;

            return toCheck.GetReferencedAssemblies()
                .Any( a => a.Name!.IndexOf( "System.Private.Uri", StringComparison.Ordinal ) >= 0 );
        }
    }
}
