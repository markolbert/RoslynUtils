namespace J4JSoftware.Roslyn.Tests
{
    public class DerivedClass : BaseClass
    {
        public override bool VirtualMethod()
        {
            return !base.VirtualMethod();
        }
    }

    public class BaseClass
    {
        public virtual bool VirtualMethod() => true;
    }
}