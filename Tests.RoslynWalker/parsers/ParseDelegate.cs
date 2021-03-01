using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseDelegate : ParseBase<DelegateInfo>
    {
        // first group is accessibility clause, second group is return-type-typeargs,
        // third group is method args
        private static readonly Regex _rxGroup = new( @$"\s*({AccessibilityClause})?(.*)\((.*)\)",
            RegexOptions.Compiled );

        public ParseDelegate()
            : base( ElementNature.Interface, 
                @"(.*\s+delegate|^delegate)\s+",
                LineType.Statement )
        {
        }

        protected override List<DelegateInfo>? Parse( StatementLine srcLine )
        {
            var groupMatch = _rxGroup.Match(srcLine.Line);

            if (!groupMatch.Success
                || groupMatch.Groups.Count != 4)
                return null;

            var methodSource = ParseReturnTypeName( groupMatch.Groups[ 2 ]
                .Value
                .Replace( "delegate ", string.Empty )
                .Trim() );

            var delegateSrc = new DelegateSource( methodSource.Name,
                groupMatch.Groups[1].Value.Trim(),
                methodSource.TypeArguments,
                ParseArguments( groupMatch.Groups[ 3 ].Value.Trim() ) );

            var info = new DelegateInfo( delegateSrc )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };

            return new List<DelegateInfo> { info };
        }
    }
}