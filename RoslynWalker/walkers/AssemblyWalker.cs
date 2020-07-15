using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn.walkers
{
    public class AssemblyWalker : SemanticWalker<IAssemblySymbol>
    {
        private static readonly List<SyntaxKind> _childKinds = new List<SyntaxKind>();

        static AssemblyWalker()
        {
            _childKinds.Add( SyntaxKind.NamespaceKeyword );
            _childKinds.Add( SyntaxKind.TypeKeyword );
            _childKinds.Add( SyntaxKind.MethodKeyword );
            _childKinds.Add( SyntaxKind.ParamKeyword );
            _childKinds.Add( SyntaxKind.ParamsKeyword );
            _childKinds.Add( SyntaxKind.FieldKeyword );
            _childKinds.Add( SyntaxKind.TypeParameter );
        }

        public AssemblyWalker( ISymbolSink symbolSink, IJ4JLogger logger ) 
            : base( symbolSink, logger )
        {
        }

        protected override bool ProcessNode( SyntaxNode node, CompilationResult context, out IAssemblySymbol? result )
        {
            result = null;

            // if the SyntaxNode is referring to an assembly within our scope, process it
            if( node.IsKind( SyntaxKind.AssemblyKeyword ) )
            {
                if( !context.GetSymbol<IAssemblySymbol>( node, out var retVal ) )
                    return false;

                result = retVal!;
            }
            else
            {
                // for all other SyntaxNodes get its ISymbol and see if it's contained in an 
                // assembly outside our scope, in which case process it
                if( context.GetSymbol<ISymbol>( node, out var otherSymbol ) )
                {
                    if( !AssemblyInScope( otherSymbol!.ContainingAssembly ) )
                        result = otherSymbol.ContainingAssembly;
                }
            }

            return result != null;
        }

        protected override List<SyntaxNode> GetTraversableChildren( SyntaxNode node )
        {
            // we're interested in traversing almost everything that's within scope
            return node.ChildNodes().ToList();
        }
    }
}
