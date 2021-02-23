using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using J4JSoftware.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Sdk;
#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class ParserCollection
    {
        private class ParserSet
        {
            public ParserFocus Focus { get; set; }
            public List<IParse> Parsers { get; set; }
            public Regex Filter { get; set; }
        }

        private readonly List<IParse> _parsers;
        private Dictionary<ParserFocus, Regex> _filters = new();
        private readonly ParserFocus[] _focusSequence =
            { ParserFocus.CurrentSourceLine, ParserFocus.FirstChildSourceLine, ParserFocus.DefaultParser };

        public ParserCollection()
        {
            _parsers = new List<IParse>
            {
                new ParseUsing(),
                new ParseNamespace(),
                new ParseClass(),
                new ParseInterface(),
                new ParseDelegate(),
                new ParseMethod(),
                new ParseProperty(),
                new ParseEvent(),
                new ParseField()
            };

            if( _parsers.Count( p => p.Focus == ParserFocus.DefaultParser ) > 1 )
                throw new ArgumentException( $"More than one default {nameof(IParse)} object defined" );

            foreach( var focus in Enum.GetValues<ParserFocus>() )
            {
                var filter = AggregateParserFilters( focus );

                if( filter != null )
                    _filters.Add( focus, filter! );
            }
        }

        private Regex? AggregateParserFilters( ParserFocus focus )
        {
            if( _parsers.All( p => p.Focus != focus ) )
                return null;

            var filterText = _parsers.Where( p => p.Focus == focus )
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

            return new Regex( filterText, RegexOptions.Compiled );
        }

        public bool HandlesLine( SourceLine srcLine )
        {
            foreach( var focus in _focusSequence )
            {
                foreach( var kvp in _filters.Where( p => p.Key == focus ) )
                {
                    var toCheck = focus switch
                    {
                        ParserFocus.FirstChildSourceLine => srcLine.LineBlock?.Lines.FirstOrDefault(),
                        _ => srcLine
                    };

                    if( toCheck == null )
                        continue;

                    if( kvp.Value.IsMatch( toCheck.Line ) )
                        return true;
                }
            }

            return false;
        }

        public BaseInfo? Parse( SourceLine srcLine )
        {
            foreach( var focus in _focusSequence )
            {
                foreach( var parser in _parsers.Where( p => p.Focus == focus ) )
                {
                    var parsed = parser.Parse( srcLine );

                    if( parsed != null )
                        return parsed;
                }
            }

            return null;
        }
    }
}