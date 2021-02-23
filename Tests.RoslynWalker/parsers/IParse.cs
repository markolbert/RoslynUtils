using System;
using System.Collections.ObjectModel;
using J4JSoftware.Utilities;

namespace Tests.RoslynWalker
{
    public interface IParse
    {
        ReadOnlyCollection<LineType> SupportedLineTypes {get;}
        ParserFocus Focus { get; }
        bool HandlesLine(SourceLine srcLine);
        string MatchText { get; }

        BaseInfo? Parse( SourceLine srcLine );
    }
}