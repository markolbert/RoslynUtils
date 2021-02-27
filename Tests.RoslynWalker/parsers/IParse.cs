using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.RoslynWalker
{
    public interface IParse
    {
        ReadOnlyCollection<LineType> SupportedLineTypes {get;}
        bool HandlesLine(StatementLine srcLine);
        string MatchText { get; }

        List<BaseInfo>? Parse( StatementLine srcLine );
    }
}