using System.Collections.Generic;

namespace Tests.RoslynWalker
{
    public class ParseUsing : ParseBase<ElementInfo>
    {
        public ParseUsing()
            : base( ElementNature.Using, @"\s*using[\w\s]+", LineType.BlockOpener)
        {
        }

        // we don't need to parse using clauses, we just skip them
        protected override List<ElementInfo>? Parse( StatementLine srcLine ) => null;
    }
}