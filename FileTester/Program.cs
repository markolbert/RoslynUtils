using System;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace J4JSoftware.Roslyn.Testing
{
    class Program
    {
        static void Main( string[] args )
        {
            var rootDir = args == null || args.Length < 2 ? "C:/Programming" : args[ 1 ];

            var projAssetFiles = Directory.GetFiles( rootDir, "project.assets.json", SearchOption.AllDirectories )
                .Distinct()
                .ToList();

            var logger = AppServiceProvider.Instance.GetRequiredService<IJ4JLogger>();
            logger.SetLoggedType<Program>();

            var projAssets = AppServiceProvider.Instance.GetRequiredService<ProjectAssets>();

            var allOkay = true;

            foreach( var projAssetFile in projAssetFiles )
            {
                // find project file, which is assumed to be immediately above the project asset file directory
                var projDir = Path.GetDirectoryName( projAssetFile );
                projDir = Path.GetDirectoryName( projDir );

                var projFiles = Directory.GetFiles( projDir, "*.csproj" )
                    .ToList();

                switch( projFiles.Count )
                {
                    case 0:
                        Console.WriteLine($"Couldn't find project file related to '{projAssetFile}'");
                        continue;

                    case 1:
                        // desired outcome
                        break;

                    default:
                        Console.WriteLine( $"Found multiple project files related to '{projAssetFile}'" );
                        continue;
                }

                Console.WriteLine( projAssetFile );
                Console.WriteLine( new string( '=', projAssetFile.Length ) );

                if( projAssets.Initialize( projFiles[0], projAssetFile ) )
                    Console.WriteLine( "passed" );
                else allOkay = false;

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine( allOkay ? "all passed" : "errors encountered" );
        }
    }
}
