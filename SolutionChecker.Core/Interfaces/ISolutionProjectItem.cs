using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolutionChecker
{
    public interface ISolutionProjectItem
    {
        string RelativePath { get; }
        Guid TypeGuid { get; }
        Guid Guid { get; }
        string Name { get; }
    }
}
