using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SolutionChecker.Tests
{
    [TestFixture]
    class SolutionReadTests
    {
        [Test]
        public void CurrentSolutionReadTest()
        {
            string currentSolutionFile = SolutionTestHelper.GetThisSolutionPath();

            ISolution s = SolutionFactory.ReadFromSolutionFile( currentSolutionFile );

            IReadOnlyCollection<ISolutionProjectItem> projectItems = s.GetProjects( ProjectType.VISUAL_C_SHARP );

            Assert.That( projectItems.Count > 0 );
            Assert.That( projectItems.Any( p => p.Name == "SolutionChecker.Tests" ) );
            Assert.That( projectItems.Any( p => p.Name == "SolutionChecker.Core" ) );
            Assert.That( projectItems.All( p => p.RelativePath.EndsWith( ".csproj" ) ) );

            Assert.That( projectItems.All( p => File.Exists( Path.Combine(s.SolutionDirectory, p.RelativePath ) ) ), "All files exist" );

            Assert.That( projectItems.Select( p => p.Guid ), Is.Unique );
            Assert.That( projectItems.Select( p => p.GetItemType() ), Is.All.EqualTo( ProjectType.VISUAL_C_SHARP ) );
        }
    }
}
