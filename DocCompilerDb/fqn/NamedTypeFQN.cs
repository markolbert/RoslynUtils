using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class NamedTypeFQN : SyntaxNodeFQN
    {
        private readonly NamespaceFQN _nsFQN;
        private readonly TypeParameterListFQN _tplFQN;

        public NamedTypeFQN(
            NamespaceFQN nsFQN,
            TypeParameterListFQN tplFQN,
            IJ4JLogger? logger
        )
            : base( logger, 
                SyntaxKind.ClassDeclaration, 
                SyntaxKind.InterfaceDeclaration, 
                SyntaxKind.RecordDeclaration,
                SyntaxKind.StructDeclaration )
        {
            _nsFQN = nsFQN;
            _tplFQN = tplFQN;
        }

        public override bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !base.GetFullyQualifiedName( node, out result ) )
                return false;

            if (!GetName(node, out var startName))
                return false;

            var sb = new StringBuilder(startName!);

            var curNode = node;

            while( ( curNode = curNode.Parent ) != null
                   && SupportedKinds.Any( x => x == curNode.Kind() ) )
            {
                if( !GetName( curNode, out var curName ) )
                    return false;

                sb.Insert( 0, $"{curName}." );
            }

            // if we hit a NamespaceDeclaration follow it up
            if( curNode.IsKind( SyntaxKind.NamespaceDeclaration ) )
            {
                if( !_nsFQN.GetFullyQualifiedName( curNode, out var nsName ) )
                {
                    Logger?.Error<string>( "Could not retrieve fully-qualified Namespace name for named type {0}",
                        sb.ToString() );
                    return false;
                }

                sb.Insert( 0, $"{nsName}." );
            }

            // if we have a type parameter list append its textual representation
            var tplNode = node.ChildNodes().FirstOrDefault( x => x.IsKind( SyntaxKind.TypeParameterList ) );
            
            if( tplNode != null )
            {
                if( !GetName( tplNode, out var tplText ) )
                {
                    Logger?.Error<string>( "Could not get TypeParameterList text for {0}", sb.ToString() );
                    return false;
                }

                sb.Append( tplText! );
            }
            
            result = sb.ToString();

            return !string.IsNullOrEmpty(result);
        }
    }
}