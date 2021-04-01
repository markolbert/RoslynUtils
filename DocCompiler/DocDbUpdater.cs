using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.DocCompiler
{
    public class DocDbUpdater : IDocDbUpdater
    {
        private readonly List<IEntityProcessor> _processors;
        private readonly IJ4JLogger? _logger;

        public DocDbUpdater(
            IEnumerable<IEntityProcessor> processors,
            IJ4JLogger? logger
        )
        {
            var topoSort = new Nodes<IEntityProcessor>();
            var tempProcessors = processors.ToList();

            foreach( var procInfo in tempProcessors
                .Select( p => new
                {
                    PredecessorAttributes = p.GetType()
                        .GetCustomAttributes( typeof(TopologicalPredecessorAttribute), false )
                        .Cast<TopologicalPredecessorAttribute>()
                        .ToList(),
                    HasRootAttribute = p.GetType()
                        .GetCustomAttributes(typeof(TopologicalRootAttribute), false)
                        .FirstOrDefault() != null,
                    Processor = p
                } )
                .Where( x => x.PredecessorAttributes.Any() || x.HasRootAttribute ) )
            {
                if( procInfo.HasRootAttribute )
                    topoSort.AddIndependentNode( procInfo.Processor );
                else
                {
                    foreach( var predecessorType in procInfo.PredecessorAttributes.Select(x=>x.PredecessorType) )
                    {
                        var predecessor = tempProcessors.FirstOrDefault( x => x.GetType() == predecessorType );
                        if( predecessor == null )
                            throw new ArgumentException(
                                $"Could not find EntityProcessor<,> type '{predecessorType.Name}'" );

                        topoSort.AddDependentNode( procInfo.Processor, predecessor );
                    }
                }
            }

            if( !topoSort.Sort( out var tempSorted, out var remaining ) )
                throw new ArgumentException( "Could not topologically sort EntityProcessor<,> collection" );

            _processors = tempSorted!;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public bool UpdateDatabase( IDocScanner docScanner ) =>
            _processors.All( processor => processor.UpdateDb( docScanner ) );
    }
}
