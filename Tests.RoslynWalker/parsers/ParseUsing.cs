using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseUsing : ParseBase<ElementInfo>
    {
        public ParseUsing()
            : base( ElementNature.Using, @"\s*using[\w\s]+", ParserFocus.CurrentSourceLine, LineType.BlockOpener)
        {
        }

        // we don't need to parse using clauses, we just skip them
        protected override List<ElementInfo>? Parse( SourceLine srcLine ) => null;
    }
}