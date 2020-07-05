namespace J4JSoftware.Roslyn.Deprecated
{
    public interface IInitializeFromNamed<in TContainer>
    {
        public bool Initialize( string rawName, TContainer container, ProjectAssetsContext context );
    }
}