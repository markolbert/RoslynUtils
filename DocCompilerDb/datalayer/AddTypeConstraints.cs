#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace J4JSoftware.DocCompiler
{
    [TopologicalPredecessor(typeof(AddBaseTypes))]
    public class AddTypeConstraints : EntityProcessor<NodeContext>
    {
        public static SyntaxKind[] SupportedKinds = new[]
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.StructDeclaration
        };

        private readonly NamedTypeFQN _ntFQN;
        private readonly TypeParameterListFQN _tplFQN;
        private readonly ITypeNodeAnalyzer _nodeAnalayzer;
        private readonly ITypeReferenceResolver _typeResolver;

        public AddTypeConstraints( 
            IFullyQualifiedNames fqNamers,
            NamedTypeFQN ntFQN,
            TypeParameterListFQN tplFQN,
            ITypeNodeAnalyzer nodeAnalzyer,
            ITypeReferenceResolver typeResolver,
            DocDbContext dbContext, 
            IJ4JLogger? logger ) 
            : base( fqNamers, dbContext, logger )
        {
            _ntFQN = ntFQN;
            _tplFQN = tplFQN;
            _nodeAnalayzer = nodeAnalzyer;
            _typeResolver = typeResolver;
        }

        protected override IEnumerable<NodeContext> GetNodesToProcess( IDocScanner source )
        {
            foreach( var scannedFile in source.ScannedFiles )
            {
                foreach( var nsNode in scannedFile.RootNode.DescendantNodes()
                    .Where( n => SupportedKinds.Any( x => x == n.Kind() )
                                 && n.ChildNodes()
                                     .Any( x => x.IsKind( SyntaxKind.TypeParameterConstraintClause ) ) )
                )
                {
                    yield return new NodeContext( nsNode, scannedFile );
                }
            }
        }

        protected override bool ProcessEntity( NodeContext nodeContext )
        {
            if( !_ntFQN.GetFullyQualifiedNameWithoutTypeParameters(nodeContext.Node, out var nonGenericName))
                return false;

            var typeParametersInfo = _tplFQN.GetTypeParameterInfo(
                nodeContext.Node.ChildNodes()
                    .FirstOrDefault( x => x.IsKind( SyntaxKind.TypeParameterList ) )
            );

            var dtDb = DbContext.DocumentedTypes
                .Include(x=>x.TypeParameters  )
                .FirstOrDefault( x => x.FullyQualifiedNameWithoutTypeParameters == nonGenericName
                                      && x.NumTypeParameters == typeParametersInfo.Count );

            if( dtDb == null )
            {
                Logger?.Error( "Could not find generic type '{0}' with '{1}' type parameters in the database",
                    nonGenericName, 
                    typeParametersInfo.Count );

                return false;
            }

            foreach( var tpDb in dtDb.TypeParameters ?? Enumerable.Empty<TypeParameter>() )
            {
                var tpi = typeParametersInfo.FirstOrDefault( x => x.Name == tpDb.Name );

                if( tpi == null )
                {
                    Logger?.Error<string, string>(
                        "Could not find reference to type parameter '{0}' (entity '{1}') in source code",
                        tpDb.Name, 
                        dtDb.FullyQualifiedName );

                    return false;
                }

                if( !UpdateTypeParameter( dtDb, nodeContext.ScannedFile, tpDb, tpi ) )
                    return false;

                DbContext.SaveChanges();
            }

            return true;
        }

        private bool UpdateTypeParameter( DocumentedType dtContextDb, IScannedFile scannedFile, TypeParameter tpDb, TypeParameterInfo tpi )
        {
            tpDb.Index = tpi.Index;

            var tcNodes = tpi.TypeConstraintNode?
                .ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TypeConstraint ) )
                .ToList() ?? new List<SyntaxNode>();

            if( !tcNodes.Any() )
                return true;

            foreach( var tci in tcNodes )
            {
                if( !_nodeAnalayzer.Analyze( tci, dtContextDb, scannedFile, true ) )
                    return false;

                if( !_typeResolver.Resolve( _nodeAnalayzer, dtContextDb, null, out var typeRef ) )
                    return false;

                var tcDb = new TypeConstraint { TypeParameterID = tpDb.ID };

                if( typeRef!.ID == 0 )
                    tcDb.ConstrainingTypeReference = typeRef;
                else tcDb.ConstrainingTypeReferenceID = typeRef.ID;

                DbContext.TypeConstraints.Add( tcDb );
            }

            return true;
        }
    }
}