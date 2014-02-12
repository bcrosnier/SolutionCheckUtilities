using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CK.Monitoring;
using NUnit.Framework;

namespace SolutionChecker.Tests
{
    [ExcludeFromCodeCoverage]
    public static class MonitoringHelper
    {
        public static GrandOutput PrepareNewGrandOutputFolder( string grandOutputFolderName = "GrandOutputDefault", int entriesPerFile = 20000 )
        {
            GrandOutput go = new GrandOutput();

            GrandOutputConfiguration c = new GrandOutputConfiguration();
            Assert.That( c.Load( CreateGrandOutputConfiguration( grandOutputFolderName, entriesPerFile ),
                TestHelper.ConsoleMonitor ) );

            Assert.That( go.SetConfiguration( c ) );

            return go;
        }

        public static XElement CreateGrandOutputConfiguration( string grandOutputDirectoryName, int entriesPerFile )
        {
            string pathEntry = String.Format( @"Path=""./{0}/""", grandOutputDirectoryName );
            return XDocument.Parse(
                    String.Format( @"
<GrandOutputConfiguration AppDomainDefaultFilter=""Release"" >
    <Channel>
        <Add Type=""BinaryFile"" Name=""GlobalCatch"" {0} MaxCountPerFile=""{1}"" />
    </Channel>
</GrandOutputConfiguration>",
                            pathEntry, entriesPerFile ) ).Root;
        }
    }
}
