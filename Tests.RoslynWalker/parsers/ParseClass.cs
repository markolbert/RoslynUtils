using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseClass : ParseInterface
    {
        public ParseClass()
            : base( ElementNature.Class, @"\s*class\s+", ParserFocus.CurrentSourceLine, LineType.BlockOpener )
        {
        }

        protected override ClassInfo? Parse( SourceLine srcLine )
        {
            return !ExtractNamedTypeArguments( srcLine.Line, "class", out var ntSource )
                ? null
                : new ClassInfo( ntSource! )
                {
                    Parent = GetParent( srcLine, ElementNature.Namespace, ElementNature.Class )
                };
        }
    }
}