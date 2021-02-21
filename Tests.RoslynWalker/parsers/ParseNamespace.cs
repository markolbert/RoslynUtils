﻿using System;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseNamespace : ParseBase<NamespaceInfo>
    {
        private static readonly Regex RxNamespaceGroup = new(@"\s*(namespace)\s+([^\s]*)", RegexOptions.Compiled);


        public ParseNamespace()
            : base( ElementNature.Namespace, @"\s*namespace\s+")
        {
        }

        public override NamespaceInfo? Parse( SourceLine srcLine )
        {
            var toProcess = GetSourceLineToProcess( srcLine );

            var match = RxNamespaceGroup.Match( toProcess.Line );

            if (!match.Success
                || match.Groups.Count != 3)
                return null;

            // namespaces can be nested so look to see if we're a child of a higher-level
            // namespace
            return new NamespaceInfo( match.Groups[ 2 ].Value.Trim() )
            {
                Parent = (NamespaceInfo?) GetParent( toProcess, ElementNature.Namespace )
            };
        }
    }
}