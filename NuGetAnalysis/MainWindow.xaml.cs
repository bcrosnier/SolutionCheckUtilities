using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK.Core;

namespace NuGetAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NuGetErrorsViewModel _vm;
        IActivityMonitor _m;
        public MainWindow()
        {
            _m = new ActivityMonitor();
            _m.SetMinimalFilter( LogFilter.Debug );

            _vm = new NuGetErrorsViewModel();
            this.DataContext = _vm;

            InitializeComponent();
            _m.Output.RegisterClient( this.ActivityMonitorListControl.Client );
            _m.Trace().Send( "Ready." );
            _m.Info().Send( "Ready." );

            LoadFromProgramArguments();
        }

        private void LoadFromProgramArguments()
        {
            var args = Environment.GetCommandLineArgs();
            if( args.Length < 2 ) return;
            string slnPath = args[1];

            if( !File.Exists( slnPath ) ) { MessageBox.Show( "File does not exist: " + slnPath ); }
            else
            {
                LoadSolutionFile( slnPath );
            }
        }

        private void LoadSolutionFile( string slnFilePath )
        {
            _vm.LoadSolutionFile( slnFilePath, _m );
        }
    }
}
