using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using NuGetAnalysis.Utils;
using SolutionChecker;

namespace NuGetAnalysis
{
    class NuGetErrorsViewModel : ViewModelBase
    {
        #region Fields
        string _title;
        string _status;
        string _activeLabelText;
        ISolution _loadedSolution;
        NuGetCheckResult _checkResult;
        #endregion

        #region Observable properties
        public string Status
        {
            get { return _status; }
            set
            {
                if( value != _status )
                {
                    _status = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if( value != _title )
                {
                    _title = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ActiveLabelText
        {
            get { return _activeLabelText; }
            set
            {
                if( value != _activeLabelText )
                {
                    _activeLabelText = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ISolution LoadedSolution
        {
            get { return _loadedSolution; }
            set
            {
                if( value != _loadedSolution )
                {
                    _loadedSolution = value;
                    RaisePropertyChanged();
                }
            }
        }

        public NuGetCheckResult CheckResult
        {
            get { return _checkResult; }
            set
            {
                if( value != _checkResult )
                {
                    _checkResult = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        public NuGetErrorsViewModel()
        {
            Status = "Ready.";
            Title = "Solution NuGet analysis";
        }

        public void LoadSolutionFile(string slnFilePath, IActivityMonitor monitor)
        {
            if( !File.Exists( slnFilePath ) ) throw new FileNotFoundException( "Specified file does not exist." );

            slnFilePath = Path.GetFullPath( slnFilePath );

            ISolution solution = SolutionFactory.ReadFromSolutionFile( slnFilePath );
            LoadedSolution = solution;

            Title = String.Format( "NuGet analysis - {0}", Path.GetFileName( slnFilePath ) );

            PerformChecks( monitor );
        }

        private void PerformChecks( IActivityMonitor monitor )
        {
            NuGetChecker checker = new NuGetChecker( _loadedSolution );
            Status = "Checking solution...";

            // This can probably be executed in background.
            NuGetCheckResult result = checker.Check();
            result.ComputeMismatchesIfNecessary();
            result.GetFrameworkMismatches();
            result.GetVersionMismatches();


            result.LogResult( monitor );
            CheckResult = result;
            Status = "Check complete.";

            StringBuilder sb = new StringBuilder();

            int errorCount = 0;

            if( result.FrameworkMismatches.Count == 0 )
            {
                sb.AppendLine( "All projects are using the same framework in their NuGet packages." );
            }
            else
            {
                errorCount++;
                sb.AppendLine( "Some projects are using different frameworks in their NuGet packages." );
            }
            
            if( result.VersionMismatches.Count == 0 )
            {
                sb.AppendLine( "All projects are using the same NuGet package versions." );
            }
            else
            {
                errorCount++;
                sb.AppendLine( "Some projects are using different NuGet package versions." );
            }

            if( errorCount == 0 )
            {
                sb.AppendLine( "\nNo problems." );
            }
            else
            {
                sb.AppendLine( "\nSee the relevant NuGet packages and the projects referencing them on the left side." );
            }

            ActiveLabelText = sb.ToString();
        }

    }
}
