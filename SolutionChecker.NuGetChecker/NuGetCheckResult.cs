using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using NuGet;

namespace SolutionChecker
{
    public class NuGetCheckResult
    {
        readonly ISolution _solution;
        readonly NuGetCheckParameters _parameters;
        IReadOnlyDictionary<string, IReadOnlyCollection<PackageReference>> _packageVersions;
        IReadOnlyDictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>> _projectPackages;

        Dictionary<string, IReadOnlyDictionary<ISolutionProjectItem, PackageReference>> _packageProjectVersionMismatches;


        /// <summary>
        /// Every package ID found in the solution, with the package references for this ID.
        /// Multiple references for a single ID mean multiple versions for a single ID.
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyCollection<PackageReference>> PackageVersions { get { return _packageVersions; } }

        /// <summary>
        /// All C# projects, with their NuGet package references
        /// </summary>
        public IReadOnlyDictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>> ProjectPackages { get { return _projectPackages; } }

        internal NuGetCheckResult( ISolution solution,
            NuGetCheckParameters parameters,
            IReadOnlyDictionary<string, IReadOnlyCollection<PackageReference>> packageVersions,
            IReadOnlyDictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>> projectPackages )
        {
            Debug.Assert( solution != null );
            Debug.Assert( parameters != null );
            Debug.Assert( packageVersions != null );
            Debug.Assert( projectPackages != null );

            _solution = solution;
            _parameters = parameters;
            _packageVersions = packageVersions;
            _projectPackages = projectPackages;
        }

        public ISolution CheckedSolution
        {
            get
            {
                return _solution;
            }
        }

        public bool HasMismatches
        {
            get
            {
                ComputeMismatchesIfNecessary();

                return _packageProjectVersionMismatches.Count > 0;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<ISolutionProjectItem, PackageReference>> MultiplePackagesPerId
        {
            get
            {
                ComputeMismatchesIfNecessary();

                return _packageProjectVersionMismatches;
            }
        }

        public void LogResult( IActivityMonitor m )
        {
            ComputeMismatchesIfNecessary();

            using( m.OpenInfo().Send( "NuGet reference analysis" ) )
            {
                List<Tuple<ISolutionProjectItem,PackageReference>> blacklistMatches = new List<Tuple<ISolutionProjectItem, PackageReference>>();
                List<Tuple<ISolutionProjectItem,PackageReference>> whitelistMatches = new List<Tuple<ISolutionProjectItem, PackageReference>>();

                foreach( var kvp in _projectPackages )
                {
                    var project = kvp.Key;
                    foreach( var reference in kvp.Value )
                    {
                        if( _parameters.MatchesBlacklist( project, reference ) ) blacklistMatches.Add( Tuple.Create( project, reference ) );
                        if( _parameters.MatchesWhitelist( project, reference ) ) whitelistMatches.Add( Tuple.Create( project, reference ) );
                    }
                }

                if( blacklistMatches.Count > 0 )
                {
                    using( m.OpenError().Send( "These packages are blacklisted for projects:" ) )
                    {
                        foreach( var t in blacklistMatches )
                        {
                            m.Error().Send( "Project '{0}' cannot use package '{1}'.", t.Item1.Name, t.Item2.Describe() );
                        }
                    }
                }

                if( whitelistMatches.Count > 0 )
                {
                    using( m.OpenInfo().Send( "These packages are explicitly allowed for projects:" ) )
                    {
                        foreach( var t in whitelistMatches )
                        {
                            m.Info().Send( "Project '{0}' can use package '{1}'.", t.Item1.Name, t.Item2.Describe() );
                        }
                    }
                }

                if( HasMismatches )
                {
                    using( m.OpenError().Send( "Different package references found for packages:" ) )
                    {
                        foreach( var kvp in MultiplePackagesPerId )
                        {
                            using( m.OpenError().Send( kvp.Key ) )
                            {
                                foreach( var innerkvp in kvp.Value )
                                {
                                    m.Error().Send( "Project '{0}' references '{1}'.", innerkvp.Key.Name, innerkvp.Value.Describe() );
                                }
                            }
                        }
                    }
                }
                else
                {
                    m.Info().Send( "No potential NuGet package conflicts were detected in this solution." );
                }

                using( m.OpenTrace().Send( "Package IDs and their versions" ) )
                {
                    foreach( var kvp in PackageVersions )
                    {
                        using( m.OpenTrace().Send( kvp.Key ) )
                        {
                            foreach( var packageReference in kvp.Value )
                                m.Trace().Send( packageReference.Describe() );
                        }
                    }
                }
                using( m.OpenTrace().Send( "C# projects and their package references" ) )
                {
                    foreach( var kvp in ProjectPackages )
                    {
                        using( m.OpenTrace().Send( kvp.Key.Name ) )
                        {
                            foreach( var packageReference in kvp.Value )
                                m.Trace().Send( packageReference.Describe() );
                        }
                    }
                }
            }
        }

        private void ComputeMismatchesIfNecessary()
        {
            if( _packageProjectVersionMismatches != null ) return;

            _packageProjectVersionMismatches = new Dictionary<string, IReadOnlyDictionary<ISolutionProjectItem, PackageReference>>();

            foreach( var kvp in _packageVersions )
            {
                string packageId = kvp.Key;
                var refs = kvp.Value;
                if( refs.Count > 1 )
                {
                    // More than one reference for a package ID
                    Dictionary<ISolutionProjectItem, PackageReference> projectReferences = new Dictionary<ISolutionProjectItem, PackageReference>();

                    foreach( var packageRef in refs )
                    {
                        // Get the projects with this ref
                        IEnumerable<ISolutionProjectItem> projects = _projectPackages.Where( pair => pair.Value.Any( p2 => p2.IsStrictlyEqualTo( packageRef ) ) ).Select( pair => pair.Key );
                        foreach( var project in projects )
                        {
                            if( !_parameters.MatchesWhitelist( project, packageRef ) ) projectReferences.Add( project, packageRef );
                        }
                    }
                    if( projectReferences.Count > 1 ) _packageProjectVersionMismatches.Add( packageId, projectReferences );
                }

            }
        }


    }
}
