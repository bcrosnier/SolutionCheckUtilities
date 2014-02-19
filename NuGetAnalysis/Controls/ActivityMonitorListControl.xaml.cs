using System;
using System.Collections.Generic;
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

namespace NuGetAnalysis.Controls
{
    /// <summary>
    /// Interaction logic for ActivityMonitorListControl.xaml
    /// </summary>
    public partial class ActivityMonitorListControl : UserControl
    {
        readonly ControlActivityMonitorClient _client;

        public ActivityMonitorListControl()
        {
            InitializeComponent();
            _client = new ControlActivityMonitorClient(this.LogListView);
        }

        public IActivityMonitorClient Client { get { return _client; } }

        private class ControlActivityMonitorClient : ActivityMonitorClient, IActivityMonitorClient
        {
            int _currentDepth = 0;
            ItemsControl _control;

            public override LogFilter MinimalFilter { get { return LogFilter.Debug; } }

            internal ControlActivityMonitorClient(ItemsControl control) : base()
            {
                _control = control;
            }

            #region IActivityMonitorClient Members

            protected override void OnAutoTagsChanged( CKTrait newTrait )
            {
                var levelStyle = GetLevelStyle( LogLevel.Info );
                AddText( String.Format( "AutoTags: {0}", newTrait ), levelStyle.Item2, _currentDepth, levelStyle.Item1 );
            }

            protected override void OnGroupClosed( IActivityLogGroup group, IReadOnlyList<ActivityLogGroupConclusion> conclusions )
            {
                var levelStyle = GetLevelStyle( group.GroupLevel );
                if( conclusions.Count > 0 )
                {
                    AddText( String.Format( "Conclusions: {0}", String.Join( "; ", conclusions ) ), levelStyle.Item2, _currentDepth, levelStyle.Item1 );
                }

                if( _currentDepth > 0 ) _currentDepth--;
            }

            protected override void OnGroupClosing( IActivityLogGroup group, ref List<ActivityLogGroupConclusion> conclusions )
            {
            }

            protected override void OnOpenGroup( IActivityLogGroup group )
            {
                var levelStyle = GetLevelStyle( group.GroupLevel );
                AddText( String.Format( "Group: {0}", group.GroupText ), levelStyle.Item2, _currentDepth, levelStyle.Item1 );
                _currentDepth++;
            }

            protected override void OnTopicChanged( string newTopic, string fileName, int lineNumber )
            {
                var levelStyle = GetLevelStyle( LogLevel.Info );
                AddText( String.Format( "Topic: {0}", newTopic ), levelStyle.Item2, _currentDepth, levelStyle.Item1 );
            }

            protected override void OnUnfilteredLog( ActivityMonitorLogData data )
            {
                AddLogEntry( data );
            }

            private void AddLogEntry( ActivityMonitorLogData data )
            {
                var levelStyle = GetLevelStyle( data.Level );
                AddText( data.Text, levelStyle.Item2, _currentDepth, levelStyle.Item1 );
            }

            private Tuple<FontWeight, Brush> GetLevelStyle( LogLevel l )
            {
                Brush foreground;
                FontWeight fontWeight;

                if( l.HasFlag( LogLevel.Trace ) )
                {
                    foreground = Brushes.Black;
                    fontWeight = FontWeights.Normal;
                }
                else if( l.HasFlag( LogLevel.Info ) )
                {
                    foreground = Brushes.Blue;
                    fontWeight = FontWeights.Normal;
                }
                else if( l.HasFlag( LogLevel.Warn ) )
                {
                    foreground = Brushes.Orange;
                    fontWeight = FontWeights.Normal;
                }
                else if( l.HasFlag( LogLevel.Error ) )
                {
                    foreground = Brushes.Red;
                    fontWeight = FontWeights.Normal;
                }
                else // Fatal
                {
                    foreground = Brushes.DarkRed;
                    fontWeight = FontWeights.Bold;
                }

                return Tuple.Create( fontWeight, foreground );
            }

            #endregion

            private void AddText( string text, Brush textColor, int depth, FontWeight weight )
            {
                TextBlock tb = new TextBlock();
                tb.Text = text;
                tb.Foreground = textColor;
                tb.Padding = new Thickness( 15 * depth, 0, 0, 0 );

                _control.Items.Add( tb );
            }

        }

    }
}
