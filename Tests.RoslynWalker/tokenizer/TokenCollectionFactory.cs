#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
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
using J4JSoftware.Logging;

namespace Tests.RoslynWalker
{
    public class TokenCollectionFactory : ITokenCollectionFactory
    {
        private readonly IActiveTokenEvolver _evolver;
        private readonly Func<IJ4JLogger>? _loggerFactory;

        public TokenCollectionFactory(
            IActiveTokenEvolver evolver,
            Func<IJ4JLogger>? loggerFactory )
        {
            _evolver = evolver;
            _loggerFactory = loggerFactory;
        }

        public Token.TokenCollection CreateTokenCollection( Token.TokenCollection? parent = null )
            => new( _evolver, parent, this, _loggerFactory?.Invoke() );
    }
}