using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SolutionChecker
{
    public class NuGetPackageLink
    {
        public string ProjectName { get; set; }
        public string PackageId { get; set; }
        public SemanticVersion PackageVersion { get; set; }
        public FrameworkName PackageTargetFramework { get; set; }

        public bool Matches(ISolutionProjectItem project, PackageReference p)
        {
            if( p.Id != PackageId ) return false;
            if( p.Version != PackageVersion ) return false;

            if( PackageTargetFramework != null && p.TargetFramework != PackageTargetFramework ) return false;
            if( !String.IsNullOrEmpty( ProjectName ) && project.Name != ProjectName ) return false;

            return true;
        }
    }
}
