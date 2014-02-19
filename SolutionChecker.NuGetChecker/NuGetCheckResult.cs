using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
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

        IReadOnlyDictionary<string, IReadOnlyCollection<PackageReference>>
            _packageVersions;
        IReadOnlyDictionary<ISolutionProjectItem, IReadOnlyCollection<PackageReference>>
            _projectPackages;

        Dictionary<string, IReadOnlyDictionary<ISolutionProjectItem, PackageReference>>
            _packageProjectVersionMismatches;

        IReadOnlyDictionary<string, IReadOnlyDictionary<FrameworkName, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>>
            _frameworkMismatches;
        IReadOnlyDictionary<string, IReadOnlyDictionary<SemanticVersion, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>>
            _versionMismatches;


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

        public IReadOnlyDictionary<string, IReadOnlyDictionary<ISolutionProjectItem, PackageReference>> MultipleVersionsPerPackageId
        {
            get
            {
                ComputeMismatchesIfNecessary();

                return _packageProjectVersionMismatches;
            }
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<FrameworkName, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>>
            FrameworkMismatches
        {
            get { return GetFrameworkMismatches(); }
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<SemanticVersion, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>>
            VersionMismatches
        {
            get { return GetVersionMismatches(); }
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
                        foreach( var kvp in MultipleVersionsPerPackageId )
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

        public void ComputeMismatchesIfNecessary()
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
                    if( projectReferences.Count > 1 )
                    {
                        // Sort by ascending package reference description
                        projectReferences = projectReferences.OrderBy( pair => pair.Value.Describe() ).ToDictionary( pair => pair.Key, pair => pair.Value );
                        _packageProjectVersionMismatches.Add( packageId, projectReferences );
                    }
                }

            }

            // Order outer by package ID
            _packageProjectVersionMismatches = _packageProjectVersionMismatches.OrderBy( pair => pair.Key ).ToDictionary( pair => pair.Key, pair => pair.Value );
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<FrameworkName, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>> GetFrameworkMismatches()
        {
            ComputeMismatchesIfNecessary();

            if( _frameworkMismatches != null ) return _frameworkMismatches;

            Dictionary<string, IReadOnlyDictionary<FrameworkName, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>> result =
                new Dictionary<string, IReadOnlyDictionary<FrameworkName, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>>();

            var packageIds = _packageProjectVersionMismatches.Keys;
            foreach( var packageId in packageIds )
            {
                Dictionary<FrameworkName, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>> frameworkRefs =
                    new Dictionary<FrameworkName, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>();

                var projectsToRefs = _packageProjectVersionMismatches[packageId];

                var distinctFrameworks = projectsToRefs.Select( x => x.Value.TargetFramework ).Distinct();
                if( distinctFrameworks.Count() < 2 ) continue;

                foreach( var framework in distinctFrameworks )
                {
                    List<Tuple<PackageReference, ISolutionProjectItem>> frameworkReferences = new List<Tuple<PackageReference, ISolutionProjectItem>>();

                    var r = projectsToRefs.Select( x => new { project = x.Key, reference = x.Value } ).Where( x => x.reference.TargetFramework == framework );
                    foreach( var pair in r )
                    {
                        frameworkReferences.Add( Tuple.Create( pair.reference, pair.project ) );
                    }

                    frameworkRefs.Add( framework, frameworkReferences );
                }
                frameworkRefs = frameworkRefs.OrderBy( x => x.Value.Count ).ToDictionary( x => x.Key, x => x.Value );

                result.Add( packageId, frameworkRefs );
            }

            _frameworkMismatches = result;
            return result;
        }

        public IReadOnlyDictionary<string, IReadOnlyDictionary<SemanticVersion, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>> GetVersionMismatches()
        {
            ComputeMismatchesIfNecessary();

            if( _versionMismatches != null ) return _versionMismatches;

            Dictionary<string, IReadOnlyDictionary<SemanticVersion, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>> result =
                new Dictionary<string, IReadOnlyDictionary<SemanticVersion, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>>();

            var packageIds = _packageProjectVersionMismatches.Keys;
            foreach( var packageId in packageIds )
            {
                Dictionary<SemanticVersion, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>> versionRefs =
                    new Dictionary<SemanticVersion, IReadOnlyCollection<Tuple<PackageReference, ISolutionProjectItem>>>();

                var projectsToRefs = _packageProjectVersionMismatches[packageId];

                var distinctVersions = projectsToRefs.Select( x => x.Value.Version ).Distinct().ToList();
                if( distinctVersions.Count() < 2 ) continue;

                foreach( var version in distinctVersions )
                {
                    List<Tuple<PackageReference, ISolutionProjectItem>> versionReferences = new List<Tuple<PackageReference, ISolutionProjectItem>>();

                    var r = projectsToRefs.Select( x => new { project = x.Key, reference = x.Value } ).Where( x => x.reference.Version == version );
                    foreach( var pair in r )
                    {
                        versionReferences.Add( Tuple.Create( pair.reference, pair.project ) );
                    }

                    versionRefs.Add( version, versionReferences );
                }
                versionRefs = versionRefs.OrderBy( x => x.Value.Count ).ToDictionary( x => x.Key, x => x.Value );
                result.Add( packageId, versionRefs );
            }
            _versionMismatches = result;
            return result;
        }


    }
}
