using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseClass : ParseInterface
    {
        public ParseClass()
            : base( ElementNature.Class, @"\s*class\s+" )
        {
        }

        public override ClassInfo? Parse( SourceLine srcLine )
        {
            var toProcess = GetSourceLineToProcess( srcLine );

            return !ExtractNamedTypeArguments( toProcess.Line, "class", out var ntSource )
                ? null
                : new ClassInfo( ntSource! )
                {
                    Parent = GetParent( toProcess, ElementNature.Namespace, ElementNature.Class )
                };
        }
    }
}