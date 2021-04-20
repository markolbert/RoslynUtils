using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class TypeReferenceInfo
    {
        private readonly List<TypeReferenceInfo> _children = new();

        public TypeReferenceInfo( SyntaxNode node )
        {
            if( SyntaxCollections.TupleKinds.Any( x => node.IsKind( x ) ) )
                throw new ArgumentException(
                    $"Cannot create a TypeReferenceInfo from a tuple node without specifying a name" );

            Name = node.ToString();
        }

        public TypeReferenceInfo( SyntaxNode node, string tupleName )
        {
            if( SyntaxCollections.TupleKinds.All( x => !node.IsKind( x ) ) )
                throw new ArgumentException(
                    $"Cannot create a TypeReferenceInfo from a non-tuple node when specifying a tuple name" );

            Name = tupleName;
            IsTuple = true;
        }

        public TypeReferenceInfo( SyntaxToken token )
        {
            Name = token.Text;
        }

        public TypeReferenceInfo? Parent { get; private set; }

        public string Name { get; }
        public bool IsPredefined { get; set; }
        public bool IsTuple { get; }
        public int Rank { get; set; }
        public ReadOnlyCollection<TypeReferenceInfo> Arguments => _children.AsReadOnly();

        public void AddChild( TypeReferenceInfo child )
        {
            child.Parent = this;

            _children.Add( child );
        }
    }
}
