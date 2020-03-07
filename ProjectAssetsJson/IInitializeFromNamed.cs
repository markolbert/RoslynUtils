using System.Runtime.CompilerServices;

namespace J4JSoftware.Roslyn
{
    public interface IInitializeFromNamed<in TContainer>
    {
        bool Initialize( string rawName, TContainer container );
    }
}