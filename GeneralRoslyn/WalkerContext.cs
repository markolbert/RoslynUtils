#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeneralRoslyn' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class WalkerContext : ActionsContext
    {
        public List<CompiledProject> CompiledProjects { get; } = new();

        public CompiledProject? this[ IAssemblySymbol symbol ] =>
            CompiledProjects.FirstOrDefault( cp =>
                SymbolEqualityComparer.Default.Equals( cp.AssemblySymbol, symbol ) );

        public List<IAssemblySymbol>? ProjectAssemblies { get; private set; }

        public void SetCompiledProjects( IEnumerable<CompiledProject> projects )
        {
            CompiledProjects.Clear();
            CompiledProjects.AddRange( projects );

            ProjectAssemblies = CompiledProjects.Select( cp => cp.AssemblySymbol ).Distinct().ToList();
        }

        public bool HasCompiledProject( IAssemblySymbol symbol )
        {
            return CompiledProjects.Any( cp => SymbolEqualityComparer.Default.Equals( cp.AssemblySymbol, symbol ) );
        }

        public bool InDocumentationScope( IAssemblySymbol toCheck )
        {
            return ProjectAssemblies?.Any( ma => SymbolEqualityComparer.Default.Equals( ma, toCheck ) ) ?? false;
        }
    }
}