using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn.walkers
{
    public class AssemblyWalker : SyntaxWalker<IAssemblySymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static AssemblyWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
        }

        private readonly IInScopeAssemblyProcessor _inScopeProcessor;

        public AssemblyWalker( 
            IEnumerable<ISymbolSink> symbolSinks,
            SymbolNamers symbolNamers,
            IInScopeAssemblyProcessor inScopeProcessor,
            IDefaultSymbolSink defaultSymbolSink,
            IJ4JLogger logger 
            ) 
            : base( symbolSinks, defaultSymbolSink, symbolNamers, logger )
        {
            _inScopeProcessor = inScopeProcessor;
        }

        // override the Traverse() method to synchronize the project file based metadata
        // for assemblies that are within the scope of the documentation
        public override bool Traverse( List<CompiledProject> compResults )
        {
            if( !base.Traverse( compResults ) )
                return false;

            if( !_inScopeProcessor.Initialize() )
                return false;

            if( !_inScopeProcessor.Synchronize( compResults ) )
                return false;

            return _inScopeProcessor.Cleanup();
        }

        protected override bool ShouldSinkNodeSymbol( SyntaxNode node, CompiledFile context, out IAssemblySymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            // SyntaxKind.CompilationUnit appears to represent the assembly level...but you can't
            // retrieve its ISymbol from the node
            if( node.IsKind( SyntaxKind.CompilationUnit ) )
            {
                Logger.Information<string, SyntaxKind>( "{0}: found {1}", 
                    context.Container.AssemblyName,
                    SyntaxKind.CompilationUnit );

                if( !SymbolIsUnProcessed( context.Container.AssemblySymbol ) ) 
                    return false;

                result = context.Container.AssemblySymbol;

                return true;
            }

            if( !context.GetSymbol<ISymbol>( node, out var otherSymbol ) )
            {
                Logger.Verbose<string, SyntaxKind>( "{0}: no ISymbol found for node of kind {1}",
                    context.Container.AssemblyName,
                    node.Kind() );

                return false;
            }

            var otherAssembly = otherSymbol!.ContainingAssembly;

            if( otherAssembly == null )
            {
                Logger.Verbose<string>( "Symbol {0} isn't contained in an Assembly", otherSymbol.ToDisplayString() );

                return false;
            }

            if( AssemblyInScope( otherAssembly ) )
            {
                Logger.Verbose<string>("Assembly for symbol {0} is in scope", otherSymbol.ToDisplayString());

                return false;
            }

            if( !SymbolIsUnProcessed( otherAssembly ) ) 
                return false;

            result = otherAssembly;

            return true;
        }

        protected override bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result )
        {
            result = null;

            // we're interested in traversing almost everything that's within scope
            // except for node types that we know don't lead any place interesting
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            result = node.ChildNodes()
                .Where(n => _ignoredNodeKinds.All(i => i != n.Kind()))
                .ToList();

            return true;
        }
    }
}
