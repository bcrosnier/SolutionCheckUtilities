// Guids.cs
// MUST match guids.h
using System;

namespace SolutionCheckUtilities.VSPackage
{
    static class GuidList
    {
        public const string guidSolutionCheckUtilities_VSPackagePkgString = "b361c6db-593a-4a32-a0cb-c5641be8a8b3";
        public const string guidSolutionCheckUtilities_VSPackageCmdSetString = "f31de0b9-c1f1-461f-b169-123919980441";

        public static readonly Guid guidSolutionCheckUtilities_VSPackageCmdSet = new Guid(guidSolutionCheckUtilities_VSPackageCmdSetString);
    };
}