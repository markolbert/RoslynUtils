using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class FullyQualifiedNamers : IFullyQualifiedNamers
    {
        private readonly Dictionary<Type, IFullyQualifiedName> _nonNodeNamers;
        private readonly Dictionary<SyntaxKind, IFullyQualifiedNameSyntaxNode> _nodeNamers = new();

        private readonly IJ4JLogger? _logger;

        delegate bool nameDelegate<in T>(T input, out string? output);

        public FullyQualifiedNamers(
            IEnumerable<IFullyQualifiedName> namers,
            IJ4JLogger? logger
        )
        {
            _logger = logger;
            _logger?.SetLoggedType(GetType());

            var temp = namers.ToList();

            _nonNodeNamers = temp
                .Where(x => x.SupportedType != typeof(SyntaxNode))
                .ToDictionary(x => x.SupportedType, x => x);

            foreach( var syntaxNamer in temp
                .Where( x => x.SupportedType == typeof( SyntaxNode ) )
                .Cast<IFullyQualifiedNameSyntaxNode>() )
            {
                foreach( var supportedKind in syntaxNamer.SupportedKinds )
                {
                    if( _nodeNamers.ContainsKey( supportedKind ) )
                        _logger?.Error( "Skipping duplicate IFullyQualifiedNameSyntaxNode for {0}", supportedKind );
                    else _nodeNamers.Add( supportedKind, syntaxNamer );
                }
            }
        }

        public bool GetName<TName>( TName entity, out string? result )
            => GetNameInternal( entity, false, out result );

        public bool GetFullyQualifiedName<TName>( TName entity, out string? result )
            => GetNameInternal( entity, true, out result );

        private bool GetNameInternal<TName>(TName entity, bool fullyQualified, out string? result)
        {
            nameDelegate<TName> namer = ( TName input, out string? output ) => { 
                output = null;
                return false;
            };

            switch( entity )
            {
                case IProjectInfo projInfo:
                    var projInfoType = projInfo.GetType();

                    if( _nonNodeNamers.ContainsKey( projInfoType ) )
                        if( fullyQualified )
                            namer = ( TName input, out string? output ) =>
                                !_nonNodeNamers[ projInfoType ].GetName( input!, out output );
                        else
                            namer = ( TName input, out string? output ) =>
                                !_nonNodeNamers[ projInfoType ].GetFullyQualifiedName( input!, out output );

                    break;

                case IScannedFile scannedFile:
                    var scannedFileType = scannedFile.GetType();

                    if (_nonNodeNamers.ContainsKey(scannedFileType))
                        if (fullyQualified)
                            namer = (TName input, out string? output) =>
                                !_nonNodeNamers[scannedFileType].GetName(input!, out output);
                        else
                            namer = (TName input, out string? output) =>
                                !_nonNodeNamers[scannedFileType].GetFullyQualifiedName(input!, out output);

                    break;

                case SyntaxNode node:
                    var nodeKind = node.Kind();

                    if (_nodeNamers.ContainsKey(nodeKind))
                        if (fullyQualified)
                            namer = (TName input, out string? output) =>
                                !_nodeNamers[nodeKind].GetName(input!, out output);
                        else
                            namer = (TName input, out string? output) =>
                                !_nodeNamers[nodeKind].GetFullyQualifiedName(input!, out output);

                    break;

                default:
                    _logger?.Error("Unsupported entity type '{0}'", typeof(TName));
                    break;

            }

            return namer( entity, out result );
        }
    }
}
