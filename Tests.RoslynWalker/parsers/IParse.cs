namespace Tests.RoslynWalker
{
    public interface IParse
    {
        bool HandlesLine(SourceLine srcLine);
        string MatchText { get; }
        bool SkipOnMatch { get; }

        BaseInfo? Parse(SourceLine srcLine, ElementNature nature);
    }

    public interface IParse<out TElement> : IParse
        where TElement : BaseInfo
    {
        TElement? Parse( SourceLine srcLine );
    }
}