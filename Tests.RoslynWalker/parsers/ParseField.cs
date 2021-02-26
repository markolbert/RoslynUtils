using System;
using System.Collections.Generic;
using System.Linq;
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
        private static readonly Regex _rxName = new( @"(\w+)(?:\s*=\s*)?(\w+)", RegexOptions.Compiled );
        private static readonly Regex _rxClass = new( @"(.*\s+class|^class)\s+", RegexOptions.Compiled );

        public ParseField()
            : base( ElementNature.Field, 
                $@"\s*({AccessibilityClause})?.+",
                ParserFocus.DefaultParser,
                LineType.Statement )
        {
        }

        public override bool HandlesLine( SourceLine srcLine )
        {
            // fields must belong to a class
            if( srcLine.Parent?.ParentLine == null
                || !_rxClass.IsMatch( srcLine.Parent.ParentLine.Line ) )
                return false;

            return base.HandlesLine( srcLine );
        }

        protected override List<FieldInfo>? Parse( SourceLine srcLine )
        {
            return ParseGeneric( srcLine ) ?? ParseNonGeneric( srcLine );

            //var fieldSrc = new FieldSource( match.Groups[ 4 ].Value.Trim(),
            //    match.Groups[ 1 ].Value.Trim(),
            //    match.Groups[ 2 ].Value.Trim()
            //        .Split( " ", StringSplitOptions.RemoveEmptyEntries )
            //        .Last()
            //    + match.Groups[ 3 ].Value.Trim(),
            //    match.Groups[ 5 ].Value.Trim() );

            //return new FieldInfo( fieldSrc )
            //{
            //    Parent = GetParent( srcLine, ElementNature.Class )
            //};
        }

        private List<FieldInfo>? ParseGeneric( SourceLine srcLine )
        {
            var match = _rxGeneric.Match(srcLine.Line);

            if( !match.Success
                || match.Groups.Count != 4 )
                return null;

            return ParseCommon( srcLine, 
                match.Groups[ 1 ].Value.Trim(),
                $"{match.Groups[ 2 ].Value.Trim()}<{match.Groups[ 3 ].Value.Trim()}>",
                match.Groups[ 4 ].Value.Trim() );
        }

        private List<FieldInfo>? ParseNonGeneric( SourceLine srcLine )
        {
            var match = _rxNonGeneric.Match(srcLine.Line);

            if( !match.Success
                || match.Groups.Count != 3 )
                return null;

            return ParseCommon( srcLine, 
                match.Groups[ 1 ].Value.Trim(), 
                match.Groups[ 2 ].Value.Trim(),
                match.Groups[ 3 ].Value.Trim() );
        }

        private List<FieldInfo>? ParseCommon( SourceLine srcLine, string accessibility, string fieldType,
            string nameClause )
        {
            var clauseMatch = _rxFields.Match( nameClause );

            if( !clauseMatch.Success )
                return null;

            var retVal = new List<FieldInfo>();

            Match? nameMatch;

            do
            {
                nameMatch = _rxName.Match( clauseMatch.Groups[ 1 ].Value.Trim() );
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

                nameMatch = nameMatch.NextMatch();
            } while( nameMatch.Success );

            return retVal;
        }
    }
}