using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseDelegate : ParseBase<DelegateInfo>
    {
        private static readonly Regex _rxDelegateGroup =
            new(@"\s*([^()]*)\s*(delegate void)\s*([^()]+)\(\s*(.*)\)", RegexOptions.Compiled);

        public ParseDelegate()
            : base( ElementNature.Interface, 
                @"(.*\s+delegate|^delegate)\s+",
                LineType.Statement )
        {
        }

        protected override List<DelegateInfo>? Parse( StatementLine srcLine )
        {
            var groupMatch = _rxDelegateGroup.Match(srcLine.Line);

            if (!groupMatch.Success
                || groupMatch.Groups.Count != 5
                || !groupMatch.Groups[2].Value.Trim().Equals("delegate void", StringComparison.Ordinal)
                || !ExtractTypeArguments(groupMatch.Groups[3].Value.Trim(), out var tempName, out var tempTypeArgs)
            )
                return null;

            var delegateSrc = new DelegateSource(tempName!,
                groupMatch.Groups[1].Value.Trim(),
                tempTypeArgs,
                ParseArguments(groupMatch.Groups[4].Value.Trim()));

            var info = new DelegateInfo( delegateSrc )
            {
                Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
            };

            return new List<DelegateInfo> { info };
        }
    }
}