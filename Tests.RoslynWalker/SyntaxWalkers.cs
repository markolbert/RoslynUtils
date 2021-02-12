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
using System.Linq;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class SyntaxWalkers : Nodes<ISyntaxWalker>
    {
        private readonly ActionsContext _context;
        private readonly IJ4JLogger? _logger;
        private readonly List<ISymbolSink> _sinks;

        public SyntaxWalkers(
            ISymbolFullName symbolNamer,
            IDefaultSymbolSink defaultSymbolSink,
            IEnumerable<ISymbolSink> symbolSinks,
            WalkerContext context,
            Func<IJ4JLogger>? loggerFactory
        )
        {
            _sinks = symbolSinks.ToList();
            _context = context;

            _logger = loggerFactory?.Invoke();
            _logger?.SetLoggedType( GetType() );

            var node = AddIndependentNode( new AssemblyWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<IAssemblySymbol>() ) );

            node = AddDependentNode( new NamespaceWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<INamespaceSymbol>() ), node.Value );

            node = AddDependentNode( new TypeWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<ITypeSymbol>() ), node.Value );

            node = AddDependentNode( new MethodWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<IMethodSymbol>() ), node.Value );

            node = AddDependentNode( new PropertyWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<IPropertySymbol>() ), node.Value );

            node = AddDependentNode( new FieldWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<IFieldSymbol>() ), node.Value );

            node = AddDependentNode( new EventWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<IEventSymbol>() ), node.Value );

            node = AddDependentNode( new AttributeWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory?.Invoke(),
                GetSink<ISymbol>() ), node.Value );
        }

        private ISymbolSink<TSymbol>? GetSink<TSymbol>()
            where TSymbol : ISymbol
        {
            return _sinks.FirstOrDefault( x => x.SupportsSymbol( typeof(TSymbol) ) )
                as ISymbolSink<TSymbol>;
        }

        public bool Process( List<CompiledProject> projects )
        {
            var numRoots = GetRoots().Count;

            switch( numRoots )
            {
                case 0:
                    _logger?.Error( "No initial ISyntaxWalker defined" );
                    return false;

                case 1:
                    // no op; desired situation
                    break;

                default:
                    _logger?.Error( "Multiple initial ISyntaxWalkers ({0}) defined", numRoots );
                    return false;
            }

            if( !Sort( out var walkerNodes, out var remainingEdges ) )
            {
                _logger?.Error( "Couldn't topologically sort ISyntaxWalkers" );
                return false;
            }

            var allOkay = true;

            foreach( var node in walkerNodes! )
            {
                allOkay &= node.Process( projects );

                if( !allOkay && _context.StopOnFirstError )
                    break;
            }

            return allOkay;
        }
    }
}