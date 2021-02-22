using System.Collections.ObjectModel;

namespace Tests.RoslynWalker
{
    public interface IParse
    {
        ReadOnlyCollection<LineType> SupportedLineTypes {get;}
        bool TestFirstChild { get; }
        bool HandlesLine(SourceLine srcLine);
        string MatchText { get; }
        bool SkipOnMatch { get; }

        BaseInfo? Parse( SourceLine srcLine );
    }

    //public interface IParse<out TElement> : IParse
    //    where TElement : BaseInfo
    //{
    //    TElement? Parse( SourceLine srcLine );
    //}
}