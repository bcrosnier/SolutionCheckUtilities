using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CK.Core;
using SolutionChecker;

namespace SimpleNuGetAnalysis
{
    public partial class MainForm : Form
    {
        StringBuilder _logBuilder;
        ActivityMonitor _m;

        public MainForm()
        {
            _logBuilder = new StringBuilder();

            _m = new ActivityMonitor();
            _m.SetTopic( "Solution NuGet analysis" );
            _m.SetMinimalFilter( LogFilter.Debug );
            _m.Output.RegisterClient( new ActivityMonitorTextWriterClient( f => AddLine( f ) ) );
            
            InitializeComponent();

            _m.Trace().Send( "Loaded window." );
        }

        public async void RunAnalysis(string slnPath)
        {
            if( !File.Exists( slnPath ) ) throw new FileNotFoundException( "Given solution file does not exist." );

            _m.Info().Send( "Loading: {0}", slnPath );

            ISolution s = SolutionFactory.ReadFromSolutionFile( slnPath );

            NuGetCheckResult result = await ComputeResults( s );

            result.LogResult( _m );
        }

        private async Task<NuGetCheckResult> ComputeResults(ISolution s)
        {
            NuGetChecker checker = new NuGetChecker( s );

            NuGetCheckResult result = await Task.Run<NuGetCheckResult>( () => checker.Check() );

            await Task.Run( () => result.ComputeMismatchesIfNecessary() );

            return result;
        }

        public void AddLine(string s)
        {
            _logBuilder.Append( s );
            this.logTestBox.Text = _logBuilder.ToString();
        }
    }
}
