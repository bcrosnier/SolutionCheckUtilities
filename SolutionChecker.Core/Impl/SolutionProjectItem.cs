using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionChecker
{
    [DebuggerDisplay( "{Name}" )]
    internal class SolutionProjectItem : ISolutionProjectItem
    {
        #region ISolutionProjectItem Members

        public Guid TypeGuid { get; private set; }

        public Guid Guid { get; private set; }

        public string Name { get; private set; }

        public string RelativePath { get; private set; }

        #endregion ISolutionProjectItem Members

        internal SolutionProjectItem( Guid projectTypeGuid, Guid projectGuid, string projectName, string projectPath )
        {
            Guid = projectGuid;
            Name = projectName;
            TypeGuid = projectTypeGuid;
            RelativePath = projectPath;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
