using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public interface IIdentifier
    {
        string Name { get; }
    }

    public class TextIdentifier : IIdentifier
    {
        public TextIdentifier( string name )
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class SingleTokenIdentifier : IIdentifier
    {
        public SingleTokenIdentifier( SyntaxToken idToken )
        {
            IDToken = idToken;
        }

        public SyntaxToken IDToken { get; }
        public string Name => IDToken.Text;
    }

    public class ParameterIdentifier : IIdentifier
    {
        public ParameterIdentifier( SyntaxNode parameterNode )
        {
            if( !parameterNode.IsKind( SyntaxKind.Parameter ) )
                throw new SyntaxNodeException( "Invalid SyntaxNode for ParameterIdentifier", 
                    SyntaxKind.Parameter,
                    parameterNode.Kind() );

            var typeNode = parameterNode.ChildNodes()
                .FirstOrDefault( x => SyntaxCollections.TypeNodeKinds.Any( x.IsKind ) );

            TypeNode = typeNode 
                       ?? throw new SyntaxNodeException( "Could not find type node for Parameter node");

            ParameterNode = parameterNode;

            ArgumentNameToken = parameterNode.ChildTokens()
                .FirstOrDefault( x => x.IsKind( SyntaxKind.IdentifierToken ) );

            TypeModifierTokens = typeNode.ChildTokens()
                .Where( x => SyntaxCollections.ArgumentModifiers.Any( y => x.IsKind( y ) ) )
                .ToArray();
        }

        public SyntaxNode ParameterNode { get; }
        public SyntaxToken ArgumentNameToken { get; }
        public SyntaxToken[] TypeModifierTokens { get; }
        public SyntaxNode TypeNode { get; }

        public string Name
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append( string.Join( " ", TypeModifierTokens.Select( x => x.Text ) ) );

                if( sb.Length > 0 )
                    sb.Append( " " );

                sb.Append( TypeNode.ToString() );
                sb.Append( " " );
                sb.Append( ArgumentNameToken.Text );

                return sb.ToString();
            }
        }
    }
}
