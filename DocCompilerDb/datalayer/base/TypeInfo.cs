using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class TypeInfo
    {
        private readonly List<TypeInfo> _children = new();

        public TypeInfo( string name )
        {
            Name = name;
        }

        public TypeInfo( SyntaxNode node )
        {
            Name = node.ToString();
        }

        public TypeInfo? Parent { get; private set; }

        public string Name { get; }
        public bool IsPredefined { get; set; }
        public int Rank { get; set; }
        public ReadOnlyCollection<TypeInfo> Arguments => _children.AsReadOnly();

        public NamedType? DbEntity {get; set; }

        public void AddChild( TypeInfo child )
        {
            child.Parent = this;

            _children.Add( child );
        }
    }
}
