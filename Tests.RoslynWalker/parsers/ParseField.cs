using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseField : ParseBase<FieldInfo>
    {
        private static readonly Regex _rxGeneric = new(
            $@"\s*({AccessibilityClause})?(\w)+(?<generic>[<])(.*)(?<-generic>[>])\s*(.*)",
            RegexOptions.Compiled);

        private static readonly Regex _rxNonGeneric =
            new( $@"\s*({AccessibilityClause})?(\w+)\s*(.*)", RegexOptions.Compiled );

        private static readonly Regex _rxFields = new(@"\s*([^,]+),?");
        private static readonly Regex _rxName = new( @"(\w+)(?:\s*=\s*)?(\w+)?", RegexOptions.Compiled );
        private static readonly Regex _rxClass = new( @"(.*\s+class|^class)\s+", RegexOptions.Compiled );

        public ParseField()
            : base( ElementNature.Field, 
                $@"\s*({AccessibilityClause})?.+",
                LineType.Statement )
        {
        }

        public override bool HandlesLine( StatementLine srcLine )
        {
            // fields must belong to a class
            if( srcLine.Parent == null
                || !_rxClass.IsMatch( srcLine.Parent.Line ) )
                return false;

            return base.HandlesLine( srcLine );
        }

        protected override List<BaseInfo>? Parse( StatementLine srcLine )=>
            ParseGeneric( srcLine ) ?? ParseNonGeneric( srcLine );

        private List<BaseInfo>? ParseGeneric( StatementLine srcLine )
        {
            var match = _rxGeneric.Match(srcLine.Line);

            if( !match.Success
                || match.Groups.Count != 6 )
                return null;

            return ParseCommon( srcLine, 
                match.Groups[ 1 ].Value.Trim(),
                $"{match.Groups[ 2 ].Value.Trim()}<{match.Groups[ 3 ].Value.Trim()}>",
                match.Groups[ 4 ].Value.Trim() );
        }

        private List<BaseInfo>? ParseNonGeneric( StatementLine srcLine )
        {
            var match = _rxNonGeneric.Match(srcLine.Line);

            if( !match.Success
                || match.Groups.Count != 4 )
                return null;

            return ParseCommon( srcLine, 
                match.Groups[ 1 ].Value.Trim(), 
                match.Groups[ 2 ].Value.Trim(),
                match.Groups[ 3 ].Value.Trim() );
        }

        private List<BaseInfo>? ParseCommon( StatementLine srcLine, string accessibility, string fieldType,
            string nameClause )
        {
            var clauseMatch = _rxFields.Match( nameClause );

            var retVal = new List<BaseInfo>();

            while( clauseMatch.Success )
            {
                var nameMatch = _rxName.Match( clauseMatch.Groups[ 1 ].Value.Trim() );
                if( !nameMatch.Success )
                    return null;

                var src = new FieldSource( nameMatch.Groups[ 1 ].Value.Trim(),
                    accessibility,
                    fieldType,
                    nameMatch.Groups[ 2 ].Value.Trim() );

                retVal.Add( new FieldInfo( src )
                {
                    Parent = GetParent( srcLine, ElementNature.Class )
                } );

                clauseMatch = clauseMatch.NextMatch();
            }

            return retVal.Count > 0 ? retVal : null;
        }
    }
}