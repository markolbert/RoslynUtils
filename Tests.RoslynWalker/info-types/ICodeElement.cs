namespace Tests.RoslynWalker
{
    public interface ICodeElement
    {
        string Name { get; }
        Accessibility Accessibility { get; }
    }
}