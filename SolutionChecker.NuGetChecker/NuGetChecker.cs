using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SolutionChecker
{
    public class NuGetChecker
    {
        readonly ISolution _solution;
        readonly NuGetCheckParameters _parameters;

        public NuGetChecker( ISolution solution, NuGetCheckParameters parameters = null )
        {
            _solution = solution;

            if( parameters == null ) parameters = new NuGetCheckParameters();
            _parameters = parameters;
        }

        public static NuGetCheckResult CheckFromSolutionFile( string p )
        {
            ISolution solution = SolutionFactory.ReadFromSolutionFile( p );

            NuGetChecker checker = new NuGetChecker( solution );

            return checker.Check();
        }

        public NuGetCheckResult Check()
        {
            IReadOnlyCollection<ISolutionProjectItem> csProjects = _solution.GetProjects( ProjectType.VISUAL_C_SHARP );

            Dictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>> projectPackagesDict = new Dictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>>();

            Dictionary<string, List<PackageReference>> packageVersions = new Dictionary<string, List<PackageReference>>();


            // Fill dictionaries
            foreach( var project in csProjects)
            {
                var projectPackages = GetPackagesFromProject( project );

                projectPackagesDict.Add( project, projectPackages );

                foreach(var reference in projectPackages)
                {
                    List<PackageReference> existingReferences;
                    if( packageVersions.TryGetValue(reference.Id, out existingReferences))
                    {
                        if( !existingReferences.Any( r => r.IsStrictlyEqualTo(reference) ) ) existingReferences.Add( reference );
                    }
                    else
                    {
                        existingReferences = new List<PackageReference>();
                        existingReferences.Add( reference );

                        packageVersions.Add( reference.Id, existingReferences );
                    }
                }
            }

            // I love non-covariance !
            Dictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>> readOnlyPackages = new Dictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>>();
            Dictionary<string, IReadOnlyCollection<PackageReference>> readOnlyReferences = new Dictionary<string, IReadOnlyCollection<PackageReference>>();

            foreach( var kvp in projectPackagesDict )
            {
                readOnlyPackages.Add( kvp.Key, kvp.Value );
            }
            foreach( var kvp in packageVersions )
            {
                readOnlyReferences.Add( kvp.Key, kvp.Value );
            }

            NuGetCheckResult result = new NuGetCheckResult(
                _solution,
                _parameters,
                readOnlyReferences,
                readOnlyPackages
                );

            return result;
        }

        private IReadOnlyCollection<PackageReference> GetPackagesFromProject( ISolutionProjectItem project )
        {
            string projectPath = Path.Combine( _solution.SolutionDirectory, project.RelativePath );
            Debug.Assert( File.Exists( projectPath ) );

            PackageReferenceFile refFile = PackageReferenceFile.CreateFromProject( projectPath );

            return refFile.GetPackageReferences().ToList();
        }
    }
}
