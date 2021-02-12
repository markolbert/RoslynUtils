#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
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

using System;
using System.Collections.Generic;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class TypeProcessors : RoslynDbProcessors<ITypeSymbol>
    {
        public TypeProcessors(
            IRoslynDataLayer dataLayer,
            WalkerContext context,
            Func<IJ4JLogger> loggerFactory
        )
            : base( "Type processing", dataLayer, context, loggerFactory() )
        {
            var node = AddIndependentNode( new TypeAssemblyProcessor( dataLayer, context, loggerFactory() ) );
            node = AddDependentNode( new TypeInScopeAssemblyInfoProcessor( dataLayer, context, loggerFactory() ),
                node.Value );
            node = AddDependentNode( new TypeNamespaceProcessor( dataLayer, context, loggerFactory() ), node.Value );
            node = AddDependentNode( new SortedTypeProcessor( dataLayer, context, loggerFactory() ), node.Value );
            AddDependentNode( new TypeArgumentProcessor( dataLayer, context, loggerFactory() ), node.Value );
            AddDependentNode( new TypeParametricTypeProcessor( dataLayer, context, loggerFactory() ), node.Value );
            AddDependentNode( new AncestorProcessor( dataLayer, context, loggerFactory() ), node.Value );
        }

        protected override bool Initialize( List<ITypeSymbol> symbols )
        {
            if( !base.Initialize( symbols ) )
                return false;

            DataLayer.MarkSharpObjectUnsynchronized<FixedTypeDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<GenericTypeDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<ParametricTypeDb>( false );
            DataLayer.MarkSharpObjectUnsynchronized<ParametricMethodTypeDb>( false );
            DataLayer.MarkUnsynchronized<TypeAncestorDb>( false );
            DataLayer.MarkUnsynchronized<TypeArgumentDb>();

            return true;
        }
    }
}