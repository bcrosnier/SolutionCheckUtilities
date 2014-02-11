using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SolutionChecker
{
    /// <summary>
    /// Static utility helper to generate ISolution objects from solution files.
    /// </summary>
    public static class SolutionFactory
    {
        /// <summary>
        /// Regex pattern for project discovery. In match order: Project type Guid, Name, Path, and project Guid.
        /// </summary>
        /// <example>
        /// Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "BCrosnier.Utils.Net", "BCrosnier.Utils.Net\BCrosnier.Utils.Net.csproj", "{B0435029-68B7-439A-8896-8D41BC963551}"
        /// </example>
        /// <remarks>
        /// Non-literal: ^Project\(\"([^"]*)\"\) = \"([^"]*)\", \"([^"]*)\", \"([^"]*)\"$
        /// </remarks>
        private static readonly string SOLUTION_PROJECT_PATTERN = @"^Project\(\""([^""]*)\""\) = \""([^""]*)\"", \""([^""]*)\"", \""([^""]*)\""$";

        /// <summary>
        /// Read a solution (.sln) file, and create a ISolution object out of it.
        /// </summary>
        /// <param name="filePath">Path of the solution file. Must exist.</param>
        /// <returns>ISolution object</returns>
        public static ISolution ReadFromSolutionFile( string filePath )
        {
            if( String.IsNullOrEmpty( filePath ) )
                throw new ArgumentNullException( "filePath" );
            if( !File.Exists( filePath ) )
                throw new ArgumentException( "File must exist", "filePath" );

            List<ISolutionProjectItem> projectItems = ParseItemsFromSolutionFile( filePath );

            Solution solution = new Solution(filePath, projectItems);

            return solution;
        }

        /// <summary>
        /// Read solution (.sln) files from a directory, and create ISolution objects out of those.
        /// </summary>
        /// <param name="directoryPath">Path of the solution directory containing .sln files. Must exist.</param>
        /// <returns>ISolution objects</returns>
        public static IEnumerable<ISolution> ReadSolutionsFromDirectory( string directoryPath )
        {
            if( String.IsNullOrEmpty( directoryPath ) )
                throw new ArgumentNullException( "directoryPath" );
            if( !Directory.Exists( directoryPath ) )
                throw new ArgumentException( "Directory must exist", "directoryPath" );

            List<ISolution> solutions = new List<ISolution>();

            DirectoryInfo dir = new DirectoryInfo( directoryPath );

            IEnumerable<FileInfo> solutionFiles = dir.GetFiles( "*.sln", SearchOption.TopDirectoryOnly );

            foreach( FileInfo solutionFile in solutionFiles )
            {
                ISolution s = ReadFromSolutionFile( solutionFile.FullName );
                solutions.Add( s );
            }

            return solutions;
        }

        private static List<ISolutionProjectItem> ParseItemsFromSolutionFile( string filePath )
        {
            List<ISolutionProjectItem> projectItems = new List<ISolutionProjectItem>();

            StreamReader reader = File.OpenText( filePath );

            while( !reader.EndOfStream )
            {
                string line = reader.ReadLine();
                Match m = Regex.Match( line, SOLUTION_PROJECT_PATTERN );
                if( m.Success )
                {
                    Guid projectTypeGuid = Guid.Parse( m.Groups[1].Value );
                    string projectName = m.Groups[2].Value;
                    string projectPath = m.Groups[3].Value;
                    Guid projectGuid = Guid.Parse( m.Groups[4].Value );

                    SolutionProjectItem item = new SolutionProjectItem( projectTypeGuid, projectGuid, projectName, projectPath );

                    projectItems.Add( item );
                }
            }

            return projectItems;
        }
    }
}
