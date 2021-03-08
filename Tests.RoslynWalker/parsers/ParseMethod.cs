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

        public IParseReturnTypeName ReturnTypeNameParser { get; set; } = new ParseReturnTypeName();
        public IParseMethodArguments MethodArgumentParser { get; set; } = new ParseMethodArguments();
        public IParseAttribute AttributeParser { get; set; } = new ParseAttribute();

        protected override List<BaseInfo>? Parse( StatementLine srcLine )
        {
            var groupMatch = _rxGroup.Match( srcLine.Line );

            if( !groupMatch.Success
                || groupMatch.Groups.Count != 4 )
                return null;

            if( !ReturnTypeNameParser.Parse( groupMatch.Groups[ 2 ].Value.Trim(),
                out var attributeClauses,
                out var returnType,
                out var name,
                out var typeArgs ) )
                return null;

            if( !AttributeParser.Parse( attributeClauses, out var attributes ) )
                return null;

            if( !MethodArgumentParser.Parse( groupMatch.Groups[ 3 ].Value.Trim(), out var arguments ) )
                return null;

            var info = new MethodInfo( new MethodSource(
                name!,
                groupMatch.Groups[ 1 ].Value.Trim(),
                typeArgs,
                arguments, returnType!,
                attributes ) )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };

            return new List<BaseInfo> { info };
        }
    }
}