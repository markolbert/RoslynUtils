using System.Runtime.CompilerServices;

namespace J4JSoftware.Roslyn
{
    public interface IInitializeFromNamed<in TContainer>
    {
        public bool Initialize( string rawName, TContainer container, ProjectAssetsContext context );
    }
}