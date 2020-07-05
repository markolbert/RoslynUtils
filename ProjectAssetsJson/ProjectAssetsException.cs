using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssetsException<T> : Exception
        where T : class
    {
        private readonly string? _textElement;
        private readonly Type? _containerType;
        private readonly string _methodName;

        public ProjectAssetsException(string message, string methodName, string? textElement = null, Type? containerType = null)
            : base(message)
        {
            _methodName = methodName;
            _textElement = textElement;
            _containerType = containerType;
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Message);

            sb.Append( "In " );
            sb.Append( _methodName.Equals( ".ctor", StringComparison.OrdinalIgnoreCase )
                ? $"the constructor for {typeof(T)}: "
                : $"{typeof(T)}:{_methodName}():" );
            sb.Append( Message );

            if( !string.IsNullOrEmpty( _textElement ) )
            {
                if( sb.Length > 0 )
                    sb.Append( " [" );

                sb.Append( $"TextElement was '{_textElement}'" );
            }

            if (_containerType != null )
            {
                if( sb.Length > 0 )
                    sb.Append( string.IsNullOrEmpty( _textElement ) ? " [" : ", " );

                sb.Append($"ContainerType was '{_containerType}']");
            }
            else
            {
                if( !string.IsNullOrEmpty( _textElement ) )
                    sb.Append( "]" );
            }

            return sb.ToString();
        }
    }
}
