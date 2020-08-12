namespace J4JSoftware.Roslyn
{
    public interface ITypeDefinitionProcessors
    {
        bool Process( TypeProcessorContext context );
    }
}