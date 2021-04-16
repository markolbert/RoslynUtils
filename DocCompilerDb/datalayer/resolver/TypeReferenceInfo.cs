using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class TypeReferenceInfo
    {
        private readonly List<TypeReferenceInfo> _children = new();

        public TypeReferenceInfo( SyntaxNode node )
        {
            Name = node.ToString();
        }

        public TypeReferenceInfo( SyntaxToken token )
        {
            Name = token.Text;
        }

        public TypeReferenceInfo? Parent { get; private set; }

        public string Name { get; }
        public bool IsPredefined { get; set; }
        public int Rank { get; set; }
        public ReadOnlyCollection<TypeReferenceInfo> Arguments => _children.AsReadOnly();

        public void AddChild( TypeReferenceInfo child )
        {
            child.Parent = this;

            _children.Add( child );
        }
    }
}
