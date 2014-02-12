using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SolutionCheckUtilities.VSPackage
{
    public class MenuCommands
    {
        // Get the development environment of VS.
        private static readonly DTE2 DTE2 = Package.GetGlobalService( typeof( DTE ) ) as DTE2;

        private static void ThrowOpenSolutionMessage()
        {
            IVsUIShell uiShell = (IVsUIShell)Package.GetGlobalService( typeof( SVsUIShell ) );
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure( uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "SolutionCheck utilities",
                       string.Format( CultureInfo.CurrentCulture, "Open or create a solution first." ),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_CRITICAL,
                       0,        // false
                       out result ) );

            return;
        }

        internal static void AnalyzeNuGetReferencesCommand( object sender, EventArgs e )
        {
            RunSimpleNuGetAnalysis();
        }

        private static void RunSimpleNuGetAnalysis()
        {
            if( !DTE2.Solution.IsOpen )
            {
                ThrowOpenSolutionMessage();
                return;
            }

            SimpleNuGetAnalysis.MainForm form = new SimpleNuGetAnalysis.MainForm();
            form.Show();
            form.Activate();
            form.RunAnalysis( DTE2.Solution.FullName );
        }
    }
}
