// Guids.cs
// MUST match guids.h
using System;

namespace Invenietis.InvenietisTools
{
    static class GuidList
    {
        public const string guidInvenietisToolsPkgString = "e9b4ed12-1c63-4df2-83a1-200870740943";
        public const string guidInvenietisToolsCmdSetString = "8c534c04-e029-4885-9de4-c62dc2c9064f";

        public static readonly Guid guidInvenietisToolsCmdSet = new Guid(guidInvenietisToolsCmdSetString);
    };
}