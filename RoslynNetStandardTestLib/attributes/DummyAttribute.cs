using System;
// ReSharper disable ValueParameterNotUsed
#pragma warning disable 67
#pragma warning disable 8618

namespace J4JSoftware.Roslyn.Tests
{
    [AttributeUsage( validOn: AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = true )]
    public class DummyAttribute : Attribute
    {
        public event EventHandler<int> PlainEvent;

        public event EventHandler<int> CustomAddRemoveEvent
        {
            add => PlainEvent += OnCustomEventAdd;
            remove => PlainEvent -= OnCustomEventRemove;
        }

        private void OnCustomEventAdd( object? sender, int e )
        {
            throw new NotImplementedException();
        }

        private void OnCustomEventRemove( object? sender, int e )
        {
            throw new NotImplementedException();
        }

        public DummyAttribute( string arg1, Type arg2 )
        {
        }

        public int TestField;
    }
}
