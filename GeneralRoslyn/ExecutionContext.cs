using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ExecutionContext
    {
        private readonly List<CompiledProject> _compiledProjects = new List<CompiledProject>();

        public bool StopOnFirstError { get; set; }

        public List<CompiledProject> CompiledProjects => _compiledProjects;

        public void SetCompiledProjects( IEnumerable<CompiledProject> projects )
        {
            _compiledProjects.Clear();
            _compiledProjects.AddRange( projects );

            ProjectAssemblies = _compiledProjects.Select( cp => cp.AssemblySymbol ).Distinct().ToList();
        }

        public CompiledProject? this[ IAssemblySymbol symbol ] =>
                _compiledProjects.FirstOrDefault( cp =>
                    SymbolEqualityComparer.Default.Equals( cp.AssemblySymbol, symbol ) );

        public bool HasCompiledProject( IAssemblySymbol symbol ) =>
            _compiledProjects.Any( cp => SymbolEqualityComparer.Default.Equals( cp.AssemblySymbol, symbol ) );

        public List<IAssemblySymbol>? ProjectAssemblies { get; private set; }

        public bool InDocumentationScope( IAssemblySymbol toCheck )
            => ProjectAssemblies?.Any( ma => SymbolEqualityComparer.Default.Equals( ma, toCheck ) ) ?? false;
    }
}
