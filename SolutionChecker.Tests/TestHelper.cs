using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using CK.Core;
using NUnit.Framework;

namespace SolutionChecker.Tests
{
    [ExcludeFromCodeCoverage]
    static partial class TestHelper
    {
        static string _testFolder;
        static string _solutionFolder;

        static IActivityMonitor _monitor;
        static ActivityMonitorConsoleClient _console;

        static TestHelper()
        {
            _monitor = new ActivityMonitor();
            _monitor.Output.BridgeTarget.HonorMonitorFilter = false;
            _console = new ActivityMonitorConsoleClient();
            _monitor.Output.RegisterClients( _console );
        }

        public static IActivityMonitor ConsoleMonitor
        {
            get { return _monitor; }
        }

        public static bool LogsToConsole
        {
            get { return _monitor.Output.Clients.Contains( _console ); }
            set
            {
                if( value ) _monitor.Output.RegisterUniqueClient( c => c == _console, () => _console );
                else _monitor.Output.UnregisterClient( _console );
            }
        }

        public static string TestFolder
        {
            get
            {
                if( _testFolder == null ) InitalizePaths();
                return _testFolder;
            }
        }

        public static string SolutionFolder
        {
            get
            {
                if( _solutionFolder == null ) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static void CleanupTestFolder()
        {
            DeleteFolder( TestFolder, true );
        }

        public static void DeleteFolder( string directoryPath, bool recreate = false )
        {
            int tryCount = 0;
            for( ; ; )
            {
                try
                {
                    if( Directory.Exists( directoryPath ) ) Directory.Delete( directoryPath, true );
                    if( recreate )
                    {
                        Directory.CreateDirectory( directoryPath );
                        File.WriteAllText( Path.Combine( directoryPath, "TestWrite.txt" ), "Test write works." );
                        File.Delete( Path.Combine( directoryPath, "TestWrite.txt" ) );
                    }
                    return;
                }
                catch( Exception ex )
                {
                    if( ++tryCount == 20 ) throw;
                    ConsoleMonitor.Info().Send( ex, "While cleaning up directory '{0}'. Retrying.", directoryPath );
                    System.Threading.Thread.Sleep( 100 );
                }
            }
        }

        private static void InitalizePaths()
        {
            string p = new Uri( System.Reflection.Assembly.GetExecutingAssembly().CodeBase ).LocalPath;
            // => CK.XXX.Tests/bin/Debug/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/bin/
            p = Path.GetDirectoryName( p );
            // => CK.XXX.Tests/
            p = Path.GetDirectoryName( p );
            // ==> CK.XXX.Tests/TestDir
            _testFolder = Path.Combine( p, "TestDir" );
            do
            {
                p = Path.GetDirectoryName( p );
            }
            while( !File.Exists( Path.Combine( p, "SolutionCheckUtilities.sln" ) ) );
            _solutionFolder = p;

            ConsoleMonitor.Info().Send( "SolutionFolder is: {1}\r\nTestFolder is: {0}", _testFolder, _solutionFolder );
            CleanupTestFolder();
        }
    }
}
