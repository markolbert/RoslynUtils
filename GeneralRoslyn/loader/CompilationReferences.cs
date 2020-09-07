using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace J4JSoftware.Roslyn
{
    public class CompilationReferences : IEnumerable<CompilationReference>
    {
        private readonly List<CompilationReference> _loadedRefs = new List<CompilationReference>();

        public void Add( CompilationReference item )
        {
            _loadedRefs.Add( item );
        }

        public void AddRange( IEnumerable<CompilationReference> items )
        {
            foreach( var item in items )
            {
                _loadedRefs.Add( item );
            }
        }

        public bool HasLoaded { get; private set; }

        public List<Microsoft.CodeAnalysis.MetadataReference> Load()
        {
            HasLoaded = false;

            var retVal = new List<Microsoft.CodeAnalysis.MetadataReference>();

            if( _loadedRefs.Count == 0 )
                return retVal;

            var refdAssemblies = new List<string>();
            var loadedAssemblies = new List<string>();

            var context = new CompilationLoadContext( this.GetType().Assembly.Location );

            _loadedRefs.ForEach( lr =>
            {
                if( lr.Load(context) )
                {
                    retVal.Add( lr.Reference! );

                    loadedAssemblies.Add( lr.AssemblyName! );
                    refdAssemblies.AddRange( lr.ReferencedAssemblies );
                }
            } );

            // load any referenced but not yet loaded assemblies
            var junk = context.Assemblies.Select( a => a.GetName().Name ).ToList();
            foreach( var other in refdAssemblies.Except( junk )
                .Distinct()
                .Select( an => new NamedReference( an! ) ) )
            {
                if( other.Load(context) )
                    retVal.Add( other.Reference! );
            }

            HasLoaded = true;

            return retVal;
        }

        public List<string> GetAssemblyNames( bool loadedOkay )
        {
            return _loadedRefs.Where( lr => lr.Succeeded == loadedOkay 
                                            && !string.IsNullOrEmpty( lr.AssemblyName! ) )
                .Select( lr => lr.AssemblyName! )
                .ToList();
        }

        public List<ReferenceLoadResult> GetLoadResults( IEnumerable<string> reqdAssemblies )
        {
            var retVal = new List<ReferenceLoadResult>();

            if( reqdAssemblies == null )
                return retVal;

            foreach( var reqdAssembly in reqdAssemblies )
            {
                CompilationLoadStatus status;

                if( _loadedRefs.Any( lr =>
                    lr.Succeeded && lr.AssemblyName!.Equals( reqdAssembly, StringComparison.Ordinal ) ) )
                    status = CompilationLoadStatus.Loaded;
                else
                    status = _loadedRefs.Any( lr =>
                        !lr.Succeeded && lr.AssemblyName!.Equals( reqdAssembly, StringComparison.Ordinal ) )
                        ? CompilationLoadStatus.FailedToLoad
                        : CompilationLoadStatus.NotLoaded;

                retVal.Add( new ReferenceLoadResult
                {
                    Assembly = reqdAssembly,
                    Status = status
                } );
            }

            return retVal;
        }

        public IEnumerator<CompilationReference> GetEnumerator()
        {
            foreach( var item in _loadedRefs )
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}