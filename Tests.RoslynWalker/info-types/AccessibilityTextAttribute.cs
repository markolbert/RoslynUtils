using System;

namespace Tests.RoslynWalker
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class AccessibilityTextAttribute : Attribute
    {
        public AccessibilityTextAttribute( string text )
        {
            Text = text;
        }

        public string Text {get;}
    }
}