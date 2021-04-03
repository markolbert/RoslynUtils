using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    public class SyntaxNodeFQN : FullyQualifiedName<SyntaxNode>, IFullyQualifiedNameSyntaxNode
    {
        private readonly List<SyntaxKind> _supportedKinds;

        protected SyntaxNodeFQN( 
            IJ4JLogger? logger,
            params SyntaxKind[] syntaxKinds
        )
            : base( logger )
        {
            _supportedKinds = syntaxKinds.ToList();

            if( _supportedKinds.Count == 0)
                Logger?.Error("No supported SyntaxKinds defined"  );
        }

        public ReadOnlyCollection<SyntaxKind> SupportedKinds => _supportedKinds.AsReadOnly();

        public override bool GetName( SyntaxNode node, out string? result )
        {
            result = null;

            var nodeKind = node.Kind();

            if( _supportedKinds.All( x => x != nodeKind ) )
                return false;

            if( !GetIdentifierTokens( node, out var idTokens ) )
                return false;

            result = string.Join( ".", idTokens! );

            return true;
        }

        public bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<SyntaxNode>? result )
        {
            result = null;

            switch( node.Kind() )
            {
                case SyntaxKind.QualifiedName:
                case SyntaxKind.TypeParameterList:
                    result = node.DescendantNodes()
                        .Where( x => x.IsKind( SyntaxKind.IdentifierName ) );

                    break;

                default:
                    var identifierNode = node.ChildNodes()
                        .FirstOrDefault( x => x.IsKind( SyntaxKind.IdentifierName ) );

                    if( identifierNode != null )
                        result =new List<SyntaxNode> { identifierNode };

                    break;
            }

            return result != null;
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            var nodeKind = node.Kind();

            return _supportedKinds.Any( x => x == nodeKind );
        }
    }
}