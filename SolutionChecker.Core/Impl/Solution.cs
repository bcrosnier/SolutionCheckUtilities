using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionChecker
{
    public class Solution : ISolution
    {
        readonly string _solutionFilePath;
        readonly IReadOnlyCollection<ISolutionProjectItem> _projects;

        internal Solution( string slnPath, IReadOnlyCollection<ISolutionProjectItem> items )
        {
            _projects = items;
            _solutionFilePath = slnPath;
        }

        #region ISolution Members


        public IReadOnlyCollection<ISolutionProjectItem> GetProjects( ProjectType projectType )
        {
            return _projects.Where( x => SolutionUtils.GetItemType(x) == projectType ).ToList();
        }

        public string SolutionFilePath
        {
            get { return _solutionFilePath; }
        }

        public string SolutionDirectory
        {
            get { return Path.GetDirectoryName( _solutionFilePath ); }
        }

        public IReadOnlyCollection<ISolutionProjectItem> Projects
        {
            get { return _projects; }
        }

        #endregion
    }
}
