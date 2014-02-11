using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SolutionChecker
{
    public class NuGetCheckParameters
    {
        IList<NuGetPackageLink> _whitelist;
        public IList<NuGetPackageLink> Whitelist { get { return _whitelist; } }

        IList<NuGetPackageLink> _blacklist;
        public IList<NuGetPackageLink> Blacklist { get { return _blacklist; } }

        public NuGetCheckParameters()
        {
            _whitelist = new List<NuGetPackageLink>();
            _blacklist = new List<NuGetPackageLink>();
        }

        internal bool MatchesWhitelist( ISolutionProjectItem project, PackageReference package )
        {

            foreach( var item in Whitelist.Where( x => x.PackageId == package.Id && x.PackageVersion == package.Version ) )
            {
                if( item.Matches( project, package ) ) return true;
            }

            return false;
        }

        internal bool MatchesBlacklist( ISolutionProjectItem project, PackageReference package )
        {
            foreach( var item in Blacklist.Where( x => x.PackageId == package.Id && x.PackageVersion == package.Version ) )
            {
                if( item.Matches( project, package ) ) return true;
            }

            return false;
        }
    }
}
