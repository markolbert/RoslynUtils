using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.Loader;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class RequiredAssembly
    {
        internal RequiredAssembly()
        {
        }

        public string? AssemblyName { get; set; }
        public string? AssemblyPath { get; set; }
    }

    public class RequiredAssemblies
    {
        private readonly AssemblyLoadContext _loadContext;
        private readonly IJ4JLogger _logger;
        private readonly List<RequiredAssembly> _assemblies = new List<RequiredAssembly>();

        public RequiredAssemblies( Func<IJ4JLogger> loggerFactory )
        {
            _loadContext = new AssemblyLoadContext(nameof(RequiredAssemblies));

            _logger = loggerFactory();
            _logger.SetLoggedType(this.GetType());
        }

        private RequiredAssemblies( IJ4JLogger logger )
        {
            _loadContext = new AssemblyLoadContext(nameof(RequiredAssemblies));

            _logger = logger;
        }

        public RequiredAssemblies Clone()
        {
            var retVal = new RequiredAssemblies( _logger );

            retVal.AddRange( _assemblies );

            return retVal;
        }

        public ReadOnlyCollection<RequiredAssembly> Assemblies => _assemblies.AsReadOnly();

        public void Clear() => _assemblies.Clear();

        public void Add( string? name = null, string? path = null )
        {
            if( name == null && path == null )
                return;

            _assemblies.Add( new RequiredAssembly { AssemblyName = name, AssemblyPath = path } );
        }

        public void AddRange( IEnumerable<RequiredAssembly> assemblies ) => _assemblies.AddRange( assemblies );

        public List<MetadataReference> GetMetadataReferences()
        {
            var retVal = new List<MetadataReference>();

            foreach (var reqdAssembly in Assemblies)
            {
                // start by trying to load the assembly by name since the framework is much
                // cleverer than me at finding assemblies...
                if (!string.IsNullOrEmpty(reqdAssembly.AssemblyName)
                    && LoadFromAssemblyName(reqdAssembly.AssemblyName!, out var mdRef))
                {
                    retVal.Add(mdRef!);
                    continue;
                }

                // if loading by name didn't work, next try loading from the file path, if one exists
                if( !string.IsNullOrEmpty( reqdAssembly.AssemblyPath )
                    && LoadFromFilePath( reqdAssembly.AssemblyPath, out var mdRef2 ) )
                {
                    retVal.Add(mdRef2!);
                    continue;
                }

                throw ProjectAssetsException.CreateAndLog(
                    $"Failed to get {typeof(MetadataReference)} for required assembly ({reqdAssembly.AssemblyName}, {reqdAssembly.AssemblyPath})",
                    this.GetType(), 
                    _logger );
            }

            return retVal;
        }

        private bool LoadFromAssemblyName(string aName, out MetadataReference? result)
        {
            result = null;

            try
            {
                var assembly = _loadContext.LoadFromAssemblyName(new AssemblyName(aName));

                if (assembly == null)
                    return false;

                result = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(assembly.Location);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return false;
            }

            return true;
        }

        private bool LoadFromFilePath(string path, out MetadataReference? result)
        {
            result = null;

            try
            {
                var assembly = _loadContext.LoadFromAssemblyPath(path);

                result = Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(assembly.Location);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                return false;
            }

            return true;
        }
    }
}