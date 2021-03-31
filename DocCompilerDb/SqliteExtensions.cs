using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    public static class SqliteExtensions
    {
        public static PropertyBuilder<TProperty> UseOsCollation<TProperty>(
            this PropertyBuilder<TProperty> propertyBuilder )
        {
            if( !OsUtilities.IsFileSystemCaseSensitive() )
                propertyBuilder.UseCollation( SqliteCollationType.CaseInsensitiveAtoZ.SqliteCollation() );

            return propertyBuilder;
        }

        public static IEnumerable<SyntaxNode> NextSiblings( this SyntaxNode curNode )
        {
            var parent = curNode.Parent;
            if( parent == null )
                return Enumerable.Empty<SyntaxNode>();

            var idx = parent.ChildNodes()
                .Where( x => ReferenceEquals( x, curNode ) )
                .Select( ( x, i ) => i )
                .First();

            return parent.ChildNodes().Skip( idx );
        }
    }
}
