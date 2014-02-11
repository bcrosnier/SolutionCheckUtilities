using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SolutionChecker.Tests
{
    static class SolutionTestHelper
    {
        public static string GetThisSolutionPath()
        {
            string directory = Path.GetDirectoryName( new Uri( Assembly.GetExecutingAssembly().CodeBase ).LocalPath );
            bool inRoot = false;
            while( !inRoot )
            {
                var slnFiles = Directory.GetFiles( directory, "*.sln" );
                Assert.That( slnFiles.Length == 0 || slnFiles.Length == 1);
                if( slnFiles.Length == 0 )
                {
                    directory = Directory.GetParent( directory ).FullName;
                }
                else
                {
                    return slnFiles[0];
                }
                if( directory == null ) inRoot = true;
            }

            return null;
        }

        public static ISolution GetTestSolution(string testSolutionName)
        {
            string thisDirectory = Path.GetDirectoryName( GetThisSolutionPath() );

            string testSolutionDirectory = Path.Combine( thisDirectory, "SolutionTests", testSolutionName );

            Assert.That( Directory.Exists( testSolutionDirectory ) );

            var slnFiles = Directory.GetFiles( testSolutionDirectory, "*.sln" );
            Assert.That( slnFiles.Length == 1 );

            ISolution testSolution = SolutionFactory.ReadFromSolutionFile( slnFiles[0] );

            return testSolution;
        }
    }
}
