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
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;

namespace Tests.RoslynWalker
{
    public class TokenEvolver : ITokenEvolver
    {
        public TokenEvolver( 
            IEnumerable<ITokenConverter> converters, 
            IEnumerable<IModifyToken> modifiers,
            IEnumerable<ISpawnToken> spawners,
            ITokenCloser closer,
            Func<IJ4JLogger>? loggerFactory )
        {
            Converters = converters.ToList();
            Modifiers = modifiers.ToList();
            Spawners = spawners.ToList();
            Closer = closer;
            LoggerFactory = loggerFactory;
        }

        public List<ITokenConverter> Converters { get; }
        public List<IModifyToken> Modifiers { get; }
        public List<ISpawnToken> Spawners { get; }
        public ITokenCloser Closer {get;}
        public Func<IJ4JLogger>? LoggerFactory { get; }
    }
}