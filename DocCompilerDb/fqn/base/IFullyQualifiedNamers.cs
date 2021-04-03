namespace J4JSoftware.DocCompiler
{
    public interface IFullyQualifiedNamers
    {
        bool GetName<TName>( TName entity, out string? result );
        bool GetFullyQualifiedName<TName>( TName entity, out string? result );
    }
}