using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseProperty : ParseBase<PropertyInfo>
    {
        private static readonly Regex _rxGroup =
            new( @$"\s*({AccessibilityClause})?([^\[\]]*)(\[.*\])?", RegexOptions.Compiled );

        private static readonly Regex _rxProperty =
            new( @$"\s*({AccessibilityClause})?\s*([\w\[\]\,]+)\s*(\w+)", RegexOptions.Compiled );

        public ParseProperty()
            : base( ElementNature.Property, 
                @".*\s+get\s*|^get\s*$|.*\s+set\s*|^set\s*$",
                LineType.BlockOpener )
        {
        }

        public override bool HandlesLine( StatementLine srcLine )
        {
            // to determine if this is a property line we check to see if the 
            // immediate child line is "get" or "set"
            if( srcLine is not BlockLine block)
                return false;

            var toCheck = block.Children.FirstOrDefault();

            return toCheck != null 
                   && toCheck.LineType == LineType.BlockOpener 
                   && base.HandlesLine( toCheck );
        }

        protected override List<PropertyInfo>? Parse( StatementLine srcLine )
        {
            var groupMatch = _rxGroup.Match( srcLine.Line );

            if( !groupMatch.Success
                || groupMatch.Groups.Count != 4 )
                return null;

            var indexers = groupMatch.Groups[ 3 ].Value.Trim();

            var propSource = ParseReturnTypeNameClause( groupMatch.Groups[ 2 ].Value.Trim() ) with
            {
                Accessibility = groupMatch.Groups[ 1 ].Value.Trim(),
                Arguments = string.IsNullOrEmpty( indexers ) ? new List<string>() : ParseArguments( indexers[ 1..^1 ] )
            };

            var info= new PropertyInfo( propSource )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };

            return new List<PropertyInfo> { info };
        }

        private MethodSource ParseReturnTypeNameClause( string text )
        {
            var numAngleBrackets = 0;
            var sb = new StringBuilder();
            string? returnType = null;

            foreach (var curChar in text)
            {
                switch (curChar)
                {
                    case ',':
                        sb.Append(curChar);
                        break;

                    case '<':
                        sb.Append( curChar );
                        numAngleBrackets++;

                        break;

                    case '>':
                        // if we're inside a type argument clause a closing
                        // angle bracket just means we're finishing an embedded type argument
                        if( numAngleBrackets > 0)
                            sb.Append(curChar);

                        numAngleBrackets--;

                        // when the number of angle brackets goes to zero we're at the end
                        // of the return type clause
                        if( numAngleBrackets == 0 )
                        {
                            returnType = sb.ToString();
                            sb.Clear();
                        }

                        break;

                    case ' ':
                        // if we're not inside a type argument clause a space
                        // means we've found the end of the return type clause
                        if( numAngleBrackets <= 0 )
                        {
                            if( sb.Length > 0 )
                            {
                                returnType = sb.ToString();
                                sb.Clear();
                            }
                        }
                        else sb.Append( curChar );

                        break;

                    default:
                        sb.Append(curChar);
                        break;
                }
            }

            return new MethodSource( sb.ToString(), 
                string.Empty, 
                TypeArguments: new List<string>(), 
                new List<string>(),
                returnType! );
        }
    }
}