using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using CK.Monitoring;
using NuGet;
using NUnit.Framework;

namespace SolutionChecker.Tests
{
    [TestFixture]
    public class NuGetReferenceTests
    {
        IActivityMonitor _m;
        GrandOutput _mainOutput;
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            TestHelper.CleanupTestFolder();

            _m = TestHelper.ConsoleMonitor;
            SystemActivityMonitor.RootLogPath = Path.Combine( TestHelper.TestFolder, "RootLogPath" );
            _mainOutput = MonitoringHelper.PrepareNewGrandOutputFolder();

            _mainOutput.Register( _m );
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _mainOutput.Dispose();
        }

        [Test]
        public void SelfNuGetReferenceTest()
        {
            _m.SetTopic( "SelfNuGetReferenceTest" );
            NuGetCheckResult r = NuGetChecker.CheckFromSolutionFile( SolutionTestHelper.GetThisSolutionPath() );

            r.LogResult( _m );

            Assert.That( r.HasMismatches == false && r.MultiplePackagesPerId.Count == 0 );

            // Testing PackageVersions
            Assert.That( r.PackageVersions, Has.Count.AtLeast( 2 ) );
            Assert.That( r.PackageVersions.Keys, Is.Unique );
            foreach( var kvp in r.PackageVersions )
            {
                Assert.That( kvp.Value.Select( pk => pk.Id ), Is.All.EqualTo( kvp.Key ) );
                Assert.That( kvp.Value.Count == 1 );
            }
            Assert.That( r.PackageVersions.Keys, Contains.Item( "CK.Core" ) );
            Assert.That( r.PackageVersions.Keys, Contains.Item( "NuGet.Core" ) );

            // Testing ProjectPackages
            Assert.That( r.ProjectPackages, Has.Count.AtLeast( 2 ) );
            Assert.That( r.ProjectPackages.Keys, Is.Unique );
            Assert.That( r.ProjectPackages.Keys.Select( x => x.Name ), Contains.Item( "SolutionChecker.Tests" ) );
            Assert.That( r.ProjectPackages.Keys.Select( x => x.Name ), Contains.Item( "SolutionChecker.Core" ) );
        }

        [Test]
        public void InvalidSolution1Test()
        {
            _m.SetTopic( "InvalidSolution1Test" );
            // 2 projects, referencing the same NuGet package, but with different versions.
            ISolution testSolution = SolutionTestHelper.GetTestSolution( "InvalidSolution1" );

            NuGetChecker checker = new NuGetChecker( testSolution );
            NuGetCheckResult r = checker.Check();

            r.LogResult( _m );

            Assert.That( r.HasMismatches == true && r.MultiplePackagesPerId.Count == 1 );
            Assert.That( r.MultiplePackagesPerId.Keys, Is.Unique );
            Assert.That( r.MultiplePackagesPerId.Keys, Contains.Item( "CK.Core" ) );
            Assert.That( r.MultiplePackagesPerId["CK.Core"], Has.Count.AtLeast( 2 ) );
            Assert.That( r.MultiplePackagesPerId["CK.Core"].Values, Is.Unique );
            Assert.That( r.MultiplePackagesPerId["CK.Core"].Keys, Is.Unique );

            Assert.That( r.GetVersionMismatches(), Has.Count.EqualTo( 1 ) );
            Assert.That( r.GetVersionMismatches()["CK.Core"], Has.Count.EqualTo( 2 ) );

            Assert.That( r.GetFrameworkMismatches(), Has.Count.EqualTo( 0 ) );
        }

        [Test]
        public void InvalidSolution2Test()
        {
            _m.SetTopic( "InvalidSolution2Test" );
            // 2 projects, referencing the same NuGet package, but with different target frameworks.
            ISolution testSolution = SolutionTestHelper.GetTestSolution( "InvalidSolution2" );

            NuGetChecker checker = new NuGetChecker( testSolution );
            NuGetCheckResult r = checker.Check();

            r.LogResult( _m );

            Assert.That( r.HasMismatches == true && r.MultiplePackagesPerId.Count == 1 );
            Assert.That( r.MultiplePackagesPerId.Keys, Is.Unique );
            Assert.That( r.MultiplePackagesPerId.Keys, Contains.Item( "CK.Core" ) );
            Assert.That( r.MultiplePackagesPerId["CK.Core"], Has.Count.AtLeast( 2 ) );
            //Assert.That( r.MultiplePackagesPerId["CK.Core"].Values, Is.Unique ); // False: 2 package references are considered equal even with different TargetFrameworks.
            Assert.That( r.MultiplePackagesPerId["CK.Core"].Keys, Is.Unique );

            Assert.That( r.GetFrameworkMismatches(), Has.Count.EqualTo( 1 ) );
            Assert.That( r.GetFrameworkMismatches()["CK.Core"], Has.Count.EqualTo( 2 ) );

            Assert.That( r.GetVersionMismatches(), Has.Count.EqualTo( 0 ) );
        }

        [Test]
        public void InvalidSolution2WhitelistTest()
        {
            _m.SetTopic( "InvalidSolution2WhitelistTest" );
            // 2 projects, referencing the same NuGet package, but with different target frameworks.
            ISolution testSolution = SolutionTestHelper.GetTestSolution( "InvalidSolution2" );
            var parameters = new NuGetCheckParameters();
            parameters.Whitelist.Add( new NuGetPackageLink()
            {
                ProjectName = "Project2",
                PackageId = "CK.Core",
                PackageVersion = SemanticVersion.Parse( "2.8.14" ),
                PackageTargetFramework = VersionUtility.ParseFrameworkName( "net45" )
            } );

            NuGetChecker checker = new NuGetChecker( testSolution, parameters );
            NuGetCheckResult r = checker.Check();

            r.LogResult( _m );

            Assert.That( r.HasMismatches == false && r.MultiplePackagesPerId.Count == 0 );

        }
    }
}
