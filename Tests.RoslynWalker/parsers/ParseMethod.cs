using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseMethod : ParseBase<MethodInfo>
    {
        private static readonly Regex _rxGroup = new( @$"\s*({AccessibilityClause})?(.*)\((.*)\)$",
            RegexOptions.Compiled );

        public ParseMethod()
            : base( ElementNature.Method, @".*\(.*\)", LineType.Statement )
        {
        }

        protected override List<MethodInfo>? Parse( StatementLine srcLine )
        {
            if (!ExtractMethodElements(srcLine.Line, out var methodSrc))
                return null;

            var info= new MethodInfo( methodSrc! )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };

            return new List<MethodInfo> { info };
        }

        protected bool ExtractMethodElements( string text, out MethodSource? result)
        {
            result = null;

            var groupMatch = _rxGroup.Match(text);

            if (!groupMatch.Success
                || groupMatch.Groups.Count != 4)
                return false;

            var returnNameGenericSource = ParseReturnTypeName( groupMatch.Groups[ 2 ].Value.Trim() );

            result = returnNameGenericSource with
            {
                Accessibility = groupMatch.Groups[ 1 ].Value.Trim(),
                Arguments = ParseArguments( groupMatch.Groups[ 3 ].Value.Trim() )
            };

            return true;
        }
    }
}