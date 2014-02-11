using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolutionChecker
{
    /// <summary>
    /// Visual Studio Solution
    /// </summary>
    public interface ISolution
    {
        string SolutionFilePath { get; }
        string SolutionDirectory { get; }

        IReadOnlyCollection<ISolutionProjectItem> Projects { get; }
        IReadOnlyCollection<ISolutionProjectItem> GetProjects( ProjectType projectType );
    }
}
