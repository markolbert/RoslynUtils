#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'RoslynDb' is free software: you can redistribute it
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
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSink : RoslynDbSink<ITypeSymbol>
    {
        private readonly TypeSymbolContainer _symbols;
        private readonly List<string> _visited = new();

        public TypeSink(
            UniqueSymbols<ITypeSymbol> uniqueSymbols,
            ActionsContext context,
            Func<IJ4JLogger> loggerFactory,
            IEnumerable<IAction<ITypeSymbol>>? processors = null )
            : base( uniqueSymbols, context, loggerFactory(), processors )
        {
            _symbols = new TypeSymbolContainer( loggerFactory() );
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.InitializeSink( syntaxWalker ) )
                return false;

            _symbols.Clear();
            _visited.Clear();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( _processors == null )
            {
                Logger?.Error( "No processors defined for {0}", GetType() );
                return false;
            }

            var allOkay = true;

            foreach( var processor in _processors )
            {
                allOkay &= processor.Process( _symbols );

                if( !allOkay && Context.StopOnFirstError )
                    break;
            }

            return allOkay;
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, ITypeSymbol symbol )
        {
            return AddType( symbol, null );
        }

        private bool AddType( ITypeSymbol symbol, ITypeSymbol? parentSymbol )
        {
            return symbol switch
            {
                INamedTypeSymbol ntSymbol => AddNamedType( ntSymbol, null ),
                ITypeParameterSymbol tpSymbol => AddTypeParameter( tpSymbol, null ),
                IArrayTypeSymbol arraySymbol => AddArrayType( arraySymbol, null ),
                _ => unhandled()
            };

            bool unhandled()
            {
                Logger?.Error<string>(
                    "ITypeSymbol '{0}' is neither an INamedTypeSymbol, an ITypeParameterSymbol nor an IArrayTypeSymbol",
                    symbol.ToFullName() );

                return false;
            }
        }

        private bool AddNamedType( INamedTypeSymbol ntSymbol, ITypeSymbol? parentSymbol )
        {
            return ntSymbol.TypeKind switch
            {
                TypeKind.Interface => AddInterface( ntSymbol ),
                _ => AddImplementableType( ntSymbol, parentSymbol )
            };
        }

        private bool AddInterface( INamedTypeSymbol symbol )
        {
            if( symbol.TypeKind != TypeKind.Interface )
            {
                Logger?.Error<string>( "Non-interface '{0}' submitted to AddInterface()", symbol.ToFullName() );
                return false;
            }

            if( symbol.BaseType != null )
            {
                Logger?.Error<string>( "Interface '{0}' has a base type", symbol.ToFullName() );
                return false;
            }

            _symbols.AddInterfaceConnection( symbol );

            if( SymbolIsDuplicate( symbol ) )
                return true;

            // add any type parameters and type arguments
            foreach( var tpSymbol in symbol.TypeParameters )
                if( !AddType( tpSymbol, symbol ) )
                    return false;

            foreach( var taSymbol in symbol.TypeArguments.Where( x => !( x is ITypeParameterSymbol ) ) )
                if( !AddType( taSymbol, symbol ) )
                    return false;

            return true;
        }

        private bool AddImplementableType( INamedTypeSymbol symbol, ITypeSymbol? parentSymbol )
        {
            _symbols.AddNonInterfaceConnection( symbol, parentSymbol );

            if( SymbolIsDuplicate( symbol ) )
                return true;

            if( symbol.BaseType != null )
                AddImplementableType( symbol.BaseType, symbol );

            // add any interfaces
            foreach( var interfaceSymbol in symbol.AllInterfaces )
                if( !AddInterface( interfaceSymbol ) )
                    return false;

            // add any type parameters and type arguments
            foreach( var tpSymbol in symbol.TypeParameters )
            {
                if( !AddType( tpSymbol, symbol ) )
                    return false;

                // add any interfaces
                foreach( var interfaceSymbol in tpSymbol.AllInterfaces )
                    if( !AddInterface( interfaceSymbol ) )
                        return false;
            }

            foreach( var taSymbol in symbol.TypeArguments.Where( x => !( x is ITypeParameterSymbol ) ) )
            {
                if( !AddType( taSymbol, symbol ) )
                    return false;

                // add any interfaces
                foreach( var interfaceSymbol in taSymbol.AllInterfaces )
                    if( !AddInterface( interfaceSymbol ) )
                        return false;
            }

            return true;
        }

        private bool AddTypeParameter( ITypeParameterSymbol symbol, ITypeSymbol? parentSymbol )
        {
            _symbols.AddNonInterfaceConnection( symbol, parentSymbol );

            if( SymbolIsDuplicate( symbol ) )
                return true;

            if( symbol.BaseType != null )
                AddImplementableType( symbol.BaseType, symbol );

            // add any interfaces
            foreach( var interfaceSymbol in symbol.AllInterfaces )
                if( !AddInterface( interfaceSymbol ) )
                    return false;

            return true;
        }

        private bool AddArrayType( IArrayTypeSymbol symbol, ITypeSymbol? parentSymbol )
        {
            _symbols.AddNonInterfaceConnection( symbol, parentSymbol );

            if( SymbolIsDuplicate( symbol ) )
                return true;

            if( symbol.BaseType != null )
                AddImplementableType( symbol.BaseType, symbol );

            if( !AddType( symbol.ElementType, symbol ) )
                return false;

            // add any interfaces
            foreach( var interfaceSymbol in symbol.AllInterfaces )
                if( !AddInterface( interfaceSymbol ) )
                    return false;

            return true;
        }

        private bool SymbolIsDuplicate( ISymbol symbol )
        {
            // don't allow duplicate additions so we can avoid infinite loops
            var fullName = symbol.ToUniqueName();

            if( _visited.Any( x => x.Equals( fullName ) ) )
                return true;

            _visited.Add( fullName );

            return false;
        }
    }
}