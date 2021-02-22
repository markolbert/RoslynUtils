using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Sdk;

namespace Tests.RoslynWalker
{
    public class ParserCollection
    {
        private readonly Regex? _rxMainFilter;
        private readonly Regex? _rxFirstChildFilter;
        private readonly List<IParse> _parsers;

        public ParserCollection( IEnumerable<IParse> parsers )
        {
            _parsers = parsers.ToList();

            _rxMainFilter = AggregateMatchers( _parsers, false );
            _rxFirstChildFilter = AggregateMatchers( _parsers, true );
        }

        private Regex? AggregateMatchers( List<IParse> parsers, bool firstChild )
        {
            var filterText = parsers.Where( p => p.TestFirstChild == firstChild )
                .Aggregate(
                    new StringBuilder(),
                    ( sb, p ) =>
                    {
                        if( sb.Length > 0 )
                            sb.Append( "|" );

                        sb.Append( p.MatchText );

                        return sb;
                    },
                    sb => sb.ToString()
                );

            return string.IsNullOrEmpty( filterText ) ? null : new Regex( filterText, RegexOptions.Compiled );
        }

        public ReadOnlyCollection<IParse> Parsers => _parsers.AsReadOnly();

        public bool HandlesLine( SourceLine srcLine )
        {
            if( _rxMainFilter?.IsMatch( srcLine.Line ) ?? false )
                return true;

            var toCheck = srcLine.LineBlock?.Lines.FirstOrDefault();
            if( toCheck == null )
                return false;

            return _rxFirstChildFilter?.IsMatch( toCheck!.Line ) ?? false;
        }

        public BaseInfo Parse( SourceLine srcLine )
        {
            foreach( var parser in _parsers )
            {
                var parsed = parser.Parse( srcLine );

                if( parsed != null )
                    return parsed;
            }

            throw new NullReferenceException( $"Failed to parse '{srcLine.Line}'" );
        }
    }
}