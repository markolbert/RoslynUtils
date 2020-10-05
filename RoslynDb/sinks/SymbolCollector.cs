using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class SymbolCollector : ISyntaxNodeSink
    {
        private readonly List<SyntaxNode> _visitedNodes = new List<SyntaxNode>();

        private readonly TopologicallySortableCollection<ISymbol> _symbols =
            new TopologicallySortableCollection<ISymbol>();
        private readonly IJ4JLogger _logger;

        private SemanticModel _curModel;

        public SymbolCollector(
            IJ4JLogger logger
        )
        {
            _logger = logger;
            _logger.SetLoggedType( this.GetType() );
        }

        public bool InitializeSink( SemanticModel model )
        {
            _visitedNodes.Clear();
            _curModel = model;

            return true;
        }

        public bool FinalizeSink( ISingleWalker syntaxWalker )
        {
            return true;
        }

        public NodeSinkResult OutputSyntaxNode( SyntaxNode node )
        {
            // don't re-process nodes
            if (_visitedNodes.Any(vn => vn.Equals(node)))
                return NodeSinkResult.AlreadyProcessed;

            _visitedNodes.Add(node);

            var symbol = GetSymbol( node );

            if( symbol == null )
            {
                _logger.Error<string>("Couldn't find ISymbol for node {0}", node.ToString());
                return NodeSinkResult.UnsupportedSyntaxNode;
            }

            // always collect the IAssemblySymbol and INamespaceSymbol...which can be
            // null for arrays, in which case we want the IAssemblySymbol and INamespaceSymbol
            // for the array's ElementType
            if( symbol is IArrayTypeSymbol arraySymbol )
            {
                ProcessAssemblySymbol( arraySymbol.ElementType.ContainingAssembly );
                ProcessNamespaceSymbol( arraySymbol.ElementType.ContainingNamespace );
            }
            else
            {
                ProcessAssemblySymbol( symbol.ContainingAssembly );
                ProcessNamespaceSymbol( symbol.ContainingNamespace );
            }

            return symbol switch
            {
                IFieldSymbol fieldSymbol => ProcessFieldSymbol(fieldSymbol),
                IPropertySymbol propSymbol => ProcessPropertySymbol(propSymbol),
                IParameterSymbol paramSymbol => ProcessParameterSymbol(paramSymbol),
                IMethodSymbol methodSymbol => ProcessMethodSymbol(methodSymbol),
                IEventSymbol eventSymbol => ProcessEventSymbol(eventSymbol),
                _ => NodeSinkResult.UnsupportedSyntaxNode
            };
        }

        private ISymbol? GetSymbol(SyntaxNode node)
        {
            var symbolInfo = _curModel.GetSymbolInfo(node);

            return symbolInfo.Symbol ?? _curModel.GetDeclaredSymbol(node);
        }

        private void ProcessAssemblySymbol( IAssemblySymbol symbol )
        {
            _symbols.Add( symbol );
        }

        private void ProcessNamespaceSymbol( INamespaceSymbol symbol )
        {
            _symbols.Add( symbol.ContainingAssembly, symbol );
        }

        private NodeSinkResult ProcessFieldSymbol( IFieldSymbol symbol )
        {
            _symbols.Add( symbol.ContainingType, symbol );
            return NodeSinkResult.Okay;
        }

        private NodeSinkResult ProcessPropertySymbol( IPropertySymbol symbol )
        {
            _symbols.Add( symbol.ContainingType, symbol );
            return NodeSinkResult.Okay;
        }

        private NodeSinkResult ProcessParameterSymbol( IParameterSymbol symbol )
        {
            _symbols.Add( symbol.ContainingSymbol, symbol );
            return NodeSinkResult.Okay;
        }

        private NodeSinkResult ProcessMethodSymbol( IMethodSymbol symbol )
        {
            _symbols.Add( symbol.ContainingType, symbol );
            return NodeSinkResult.Okay;
        }

        private NodeSinkResult ProcessEventSymbol( IEventSymbol symbol )
        {
            _symbols.Add( symbol.ContainingType, symbol );
            return NodeSinkResult.Okay;
        }
    }
}
