using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseField : ParseBase<FieldInfo>
    {
        private static readonly Regex _rxField = new(@"\s*({_accessibilityClause})?\s*([^<>]+)(<.+>)?\s*(\w+)\s*=?\s*(.*)?");


        public ParseField()
            : base( ElementNature.Field, 
                @"\s*({_accessibilityClause})?\s*([^<>]+)(<.+>)?\s*(\w+)\s*=?\s*(.*)?",
                ParserFocus.DefaultParser,
                LineType.Statement )
        {
        }

        public override bool HandlesLine( SourceLine srcLine )
        {
            // fields must belong to a class
            if( srcLine.LineBlock == null
                || srcLine.LineBlock.ParentLine?.Line.IndexOf( "class ", StringComparison.Ordinal ) < 0 )
                return false;

            return base.HandlesLine( srcLine );
        }

        protected override FieldInfo? Parse( SourceLine srcLine )
        {
            var match = _rxField.Match(srcLine.Line);

            if( !match.Success 
                || match.Groups.Count != 6 )
                return null;

            var fieldSrc = new FieldSource( match.Groups[ 4 ].Value.Trim(),
                match.Groups[ 1 ].Value.Trim(),
                match.Groups[ 2 ].Value.Trim()
                    .Split( " ", StringSplitOptions.RemoveEmptyEntries )
                    .Last()
                + match.Groups[ 3 ].Value.Trim(),
                match.Groups[ 5 ].Value.Trim() );

            return new FieldInfo( fieldSrc )
            {
                Parent = GetParent( srcLine, ElementNature.Class )
            };
        }
    }
}