﻿#region license

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

using J4JSoftware.Logging;

namespace J4JSoftware.DocCompiler
{
    public class ProjectFile : StandaloneFile
    {
        public static bool ParseFileInProject(
            ProjectInfo projectInfo,
            string sourceFilePath,
            DocNodeWalker nodeWalker,
            IJ4JLogger? logger,
            out ProjectFile? result)
        {
            result = null;

            if( !ParseStandaloneFile( sourceFilePath, nodeWalker, logger, out var standalone ) )
                return false;

            result = new ProjectFile( projectInfo )
            {
                SourceFilePath = sourceFilePath,
                RootNode = standalone!.RootNode,
                Usings = standalone.Usings,
                Namespaces = standalone.Namespaces,
                Classes = standalone.Classes,
                Interfaces = standalone.Interfaces,
                Structs = standalone.Structs,
                Records = standalone.Records
            };

            return true;
        }

        private ProjectFile( ProjectInfo projInfo )
        {
            BelongsTo = projInfo;
        }

        public ProjectInfo BelongsTo { get; }
    }
}