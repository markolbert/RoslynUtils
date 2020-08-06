using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.processors
{
    public static class AccessibilityExtensions
    {
        public static bool IsProtected( this Accessibility access )
            => access switch
            {
                Accessibility.Protected => true,
                Accessibility.ProtectedAndInternal => true,
                Accessibility.ProtectedOrInternal => true,

                _ => false
            };

        public static bool IsPrivate( this Accessibility access )
            => access switch
            {
                Accessibility.Private => true,
                Accessibility.ProtectedAndInternal => true,

                _ => false
            };

        public static bool IsPublic(this Accessibility access)
            => access == Accessibility.Public;

        public static bool IsInternal(this Accessibility access)
            => access switch
            {
                Accessibility.Internal => true,
                Accessibility.ProtectedAndInternal => true,
                Accessibility.ProtectedOrInternal => true,

                _ => false
            };
    }
}
