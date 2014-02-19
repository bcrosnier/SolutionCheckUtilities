using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invenietis.InvenietisTools
{
    internal static class MenuCommands
    {
        // Get the development environment of VS.
        private static readonly DTE2 DTE2 = Package.GetGlobalService(typeof(DTE)) as DTE2;

        private static readonly string UTILITIES_EXECUTABLE_PATH = typeof(NuGetAnalysis.App).Assembly.Location;

        private static void ThrowOpenSolutionMessage()
        {
            IVsUIShell uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "SolutionCheck utilities",
                       string.Format(CultureInfo.CurrentCulture, "Open or create a solution first."),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_CRITICAL,
                       0,        // false
                       out result));

            return;
        }

        internal static void AnalyzeNuGetReferencesCommand(object sender, EventArgs e)
        {
            RunSimpleNuGetAnalysis();
        }

        private static void RunSimpleNuGetAnalysis()
        {
            if (!DTE2.Solution.IsOpen)
            {
                ThrowOpenSolutionMessage();
                return;
            }

            OpenUtilities(DTE2.Solution.FullName);

            //SimpleNuGetAnalysis.MainForm form = new SimpleNuGetAnalysis.MainForm();
            //form.Show();
            //form.Activate();
            //form.RunAnalysis( DTE2.Solution.FullName );
        }

        private static void OpenUtilities(string slnPath)
        {
            if (File.Exists(UTILITIES_EXECUTABLE_PATH))
            {
                string parameters = "\"" + slnPath + "\" ";
                System.Diagnostics.Process.Start(UTILITIES_EXECUTABLE_PATH, parameters);
            }
            else
            {
                IVsUIShell uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
                Guid clsid = Guid.Empty;
                int result;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                           0,
                           ref clsid,
                           "SolutionCheck utilities",
                           string.Format(CultureInfo.CurrentCulture, "Executable not found: {0}", slnPath),
                           string.Empty,
                           0,
                           OLEMSGBUTTON.OLEMSGBUTTON_OK,
                           OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                           OLEMSGICON.OLEMSGICON_CRITICAL,
                           0,        // false
                           out result));
            }
        }
    }
}
