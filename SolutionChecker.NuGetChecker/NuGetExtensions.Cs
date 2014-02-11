using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SolutionChecker
{
    public static class NuGetExtensions
    {
        public static bool IsStrictlyEqualTo( this PackageReference a, PackageReference b )
        {
            return a.Id == b.Id
                && a.Version == b.Version
                && a.TargetFramework == b.TargetFramework;
        }
        public static string Describe( this PackageReference pref )
        {
            if( pref.TargetFramework == null )
            {
                return pref.ToString();
            }
            else
            {
                return String.Format( "{0} [{1}]", pref.ToString(), pref.TargetFramework.ToString() );
            }
        }
    }
}
