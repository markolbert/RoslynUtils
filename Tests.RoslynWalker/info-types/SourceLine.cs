using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tests.RoslynWalker
{
    public class SourceLine
    {
        public static readonly string[] AccessTokens = { "public", "protected", "private", "internal", "protected internal", string.Empty };

        private Accessibility _accessibility = Accessibility.Undefined;
        private ElementNature _nature = ElementNature.Unknown;

        public SourceLine( string line, LineBlock? lineBlock )
        {
            Line = StripAttributes(line);
            LineBlock = lineBlock;
        }

        public string Line { get; }
        public LineBlock? LineBlock { get; }

        public Accessibility Accessibility
        {
            get
            {
                if( _accessibility != Accessibility.Undefined )
                    return _accessibility;

                InitializeAccessibilityAndNature();

                return _accessibility;
            }
        }

        public ElementNature Nature
        {
            get
            {
                if( _nature != ElementNature.Unknown )
                    return _nature;

                InitializeAccessibilityAndNature();

                return _nature;
            }
        }
        
        public LineBlock? ChildBlock { get; set; }

        private string StripAttributes( string line )
        {
            var startChar = 0;
            var endChar = 0;

            var sb = new StringBuilder();

            while( ( startChar = line[ startChar.. ].IndexOf( "[", StringComparison.Ordinal ) ) != 0 )
            {
                if( startChar > endChar )
                    sb.Append( line[ endChar..( startChar - 1 ) ] );

                endChar = line[ startChar.. ].IndexOf( "]", StringComparison.Ordinal );

                if( endChar < 0 )
                    throw new ArgumentException( $"Unmatched attribute opener '[' in {line}" );

                startChar = endChar + 1;

                if( startChar >= line.Length )
                    break;
            }

            if( endChar < line.Length - 1 )
                sb.Append( line[ endChar.. ] );

            var retVal = sb.ToString();

            while( retVal.IndexOf( "  ", StringComparison.Ordinal ) >= 0 )
            {
                retVal = retVal.Replace( "  ", " " );
            }

            return retVal;
        }

        private void InitializeAccessibilityAndNature()
        {
            // this default isn't strictly speaking valid...but it is (I hope!) so long as one doesn't
            // drill down more than one level below a NamedType
            _nature = ElementNature.Field;
            _accessibility = Accessibility.Private;

            foreach( var accessToken in Enum.GetValues<Accessibility>().Where( x => x != Accessibility.Undefined ) )
            {
                foreach( var accessText in accessToken.GetType()
                    .GetCustomAttributes<AccessibilityTextAttribute>( false )
                    .Select( x => x.Text ) )
                {
                    var separator = accessText.Length > 0 ? " " : string.Empty;

                    if( Line.IndexOf( $"{accessToken}{separator}delegate", StringComparison.Ordinal ) >= 0 )
                    {
                        _accessibility = accessToken;
                        _nature = ElementNature.Delegate;

                        break;
                    }

                    if( Line.IndexOf( $"{accessToken}{separator}class", StringComparison.Ordinal ) >= 0 )
                    {
                        _accessibility = accessToken;
                        _nature = ElementNature.Class;

                        break;
                    }

                    if( Line.IndexOf( $"{accessToken}{separator}interface", StringComparison.Ordinal ) >= 0 )
                    {
                        _accessibility = accessToken;
                        _nature = ElementNature.Interface;

                        break;
                    }

                    if( Line.IndexOf( $"{accessToken}{separator}event", StringComparison.Ordinal ) >= 0 )
                    {
                        _accessibility = accessToken;
                        _nature = ElementNature.Event;

                        break;
                    }

                    // a delegate would trip this but we've already handled that
                    if( Line.IndexOf( "(", StringComparison.Ordinal ) >= 0 )
                    {
                        _accessibility = accessToken;
                        _nature = ElementNature.Method;

                        break;
                    }

                    if( (LineBlock?.Lines.First().Line.Equals("get",StringComparison.Ordinal) ?? false)
                        || (LineBlock?.Lines.First().Line.Equals("set", StringComparison.Ordinal) ?? false) )
                    {
                        _accessibility = accessToken;
                        _nature = ElementNature.Property;

                        break;
                    }

                    if( separator != " "
                        && Line.IndexOf( $"{accessToken}{separator}", StringComparison.Ordinal ) >= 0 )
                    {
                        _accessibility = accessToken;
                        break;
                    }
                }
            }
        }
    }
}