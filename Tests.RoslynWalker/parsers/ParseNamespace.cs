using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseNamespace : ParseBase<NamespaceInfo>
    {
        private static readonly Regex RxNamespaceGroup = new(@"\s*(namespace)\s+([^\s]*)", RegexOptions.Compiled);


        public ParseNamespace()
            : base( ElementNature.Namespace, @"\s*namespace\s+", ParserFocus.CurrentSourceLine, LineType.BlockOpener)
        {
        }

        protected override List<NamespaceInfo>? Parse( SourceLine srcLine )
        {
            var match = RxNamespaceGroup.Match( srcLine.Line );

            if (!match.Success
                || match.Groups.Count != 3)
                return null;

            // namespaces can be nested so look to see if we're a child of a higher-level
            // namespace
            var info = new NamespaceInfo( match.Groups[ 2 ].Value.Trim() )
            {
                Parent = (NamespaceInfo?) GetParent( srcLine, ElementNature.Namespace )
            };

            return new List<NamespaceInfo> { info };
        }
    }
}