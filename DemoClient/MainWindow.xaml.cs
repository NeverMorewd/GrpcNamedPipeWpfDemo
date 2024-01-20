using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace DemoClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {     
        public MainWindow()
        {
            InitializeComponent();

            this.Title = $"{Title}-{Environment.ProcessId}";

            ViewModel = new AppViewModel();

            this.WhenActivated(dispos =>
            {
                this.Bind(ViewModel,
                    vm => vm.IsChecked,
                    vw => vw.TestCheckBox.IsChecked)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.ClientMessage,
                    vw => vw.ClientMessageTextBox.Text)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.UnaryMessage,
                    vw => vw.UnaryMessageTextBox.Text)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel,
                    vm => vm.IsChecked,
                    vw => vw.TestExpander.IsExpanded)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel, 
                    vm => vm.UnaryFacade.UnaryTracks, 
                    vw => vw.UnaryTracks.ItemsSource)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel, 
                    vm => vm.ClientStreamingFacade.Tracks, 
                    vw => vw.ClientTracks.ItemsSource)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel, 
                    vm => vm.ServerStreamingFacade.ResponseTracks, 
                    vw => vw.ServerTracks.ItemsSource)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel,
                   vm => vm.ConverseStreamingFacade.Tracks,
                   vw => vw.ConverseRequestingTracks.ItemsSource)
                   .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.UnaryFacade.IsLongRunnig,
                    vw => vw.TaskOption.IsChecked)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.UnaryFacade.Timeout,
                    vw => vw.TimeoutTextBox.Text)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                   vm => vm.UnaryFacade.InternalDelay,
                   vw => vw.InternalTextBox.Text)
                   .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.UnaryFacade.ServerDelay,
                    vw => vw.ServerDelayTextBox.Text)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                   vm => vm.ServerStreamingFacade.ServerDelay,
                   vw => vw.ServerStreamingDelayTextBox.Text)
                   .DisposeWith(dispos);

                this.Bind(ViewModel,
                      vm => vm.UnaryFacade.IsUseMutiClient,
                      vw => vw.MultiClientCheckBox.IsChecked)
                      .DisposeWith(dispos);

                this.Bind(ViewModel,
                      vm => vm.UnaryFacade.MinTimeElapsed,
                      vw => vw.TroughCost.Content)
                      .DisposeWith(dispos);

                this.Bind(ViewModel,
                     vm => vm.UnaryFacade.MaxTimeElapsed,
                     vw => vw.PeakCost.Content)
                     .DisposeWith(dispos);

                this.Bind(ViewModel,
                     vm => vm.UnaryFacade.AverageTimeElapsed,
                     vw => vw.AverageCost.Content)
                     .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.UnaryFacade.MaxCount,
                    vw => vw.MaxCount.Content)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel,
                    vm => vm.UnaryFacade.CostSeries,
                    vw => vw.CostChart.Series)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel,
                    vm => vm.UnaryFacade.ChartSyncContext,
                    vw => vw.CostChart.SyncContext)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                   vm => vm.UnaryFacade.AllCount,
                   vw => vw.AllCount.Content)
                   .DisposeWith(dispos);

                //this.Bind(ViewModel,
                //   vm => vm.TestFacade.DllPath,
                //   vw => vw.TestFacade.BrowseCommand)
                //   .DisposeWith(dispos);



                this.BindCommand(ViewModel,
                    vm => vm.UnaryOnceCommand,
                    v => v.BeepUnaryButton,
                    vm => vm.UnaryMessage)
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                    vm => vm.PushOnceCommand,
                    v => v.PushOnceButton,
                    vm => vm.ClientMessage)
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel, 
                    vm => vm.StartClientStreamingCommand, 
                    v => v.StartClientStreamingButton, 
                    nameof(StartClientStreamingButton.Checked))
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel, 
                    vm => vm.StopClientStreamingCommand, 
                    v => v.StartClientStreamingButton, 
                    nameof(StartClientStreamingButton.Unchecked))
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel, 
                    vm => vm.StartServerStreamingCommand, 
                    v => v.StartServerStreamingButton, 
                    nameof(StartServerStreamingButton.Checked))
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel, 
                    vm => vm.StopServerStreamingCommand, 
                    v => v.StartServerStreamingButton, 
                    nameof(StartServerStreamingButton.Unchecked))
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel, 
                    vm => vm.UnaryClearCommand, 
                    v => v.UnaryClearButton)
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel, 
                    vm => vm.ClientStreamingClearCommand, 
                    v => v.ClientStreamingClearButton)
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel, 
                    vm => vm.ServerStreamingClearCommand, 
                    v => v.ServerStreamingClearButton)
                   .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                   vm => vm.UnaryFacade.StartAutoUnaryCommand,
                   v => v.StartAutoUnary,
                   vm => vm.UnaryMessage,
                   nameof(StartAutoUnary.Checked))
                   .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                    vm => vm.UnaryFacade.StopAutoUnaryCommand,
                    v => v.StartAutoUnary,
                    nameof(StartAutoUnary.Unchecked))
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                    vm => vm.StartConverseStreamingCommand,
                    v => v.StartConverseRequesting,
                    nameof(StartConverseRequesting.Checked))
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                    vm => vm.StopConverseStreamingCommand,
                    v => v.StartConverseRequesting,
                    nameof(StartConverseRequesting.Unchecked))
                    .DisposeWith(dispos);


                this.BindCommand(ViewModel,
                    vm => vm.ConverseStreamingClearCommand,
                    v => v.ConverseRequestingClearButton)
                   .DisposeWith(dispos);


                #region runtimeunary

                this.Bind(ViewModel,
                   vm => vm.RuntimeUnaryFacade.IsLongRunnig,
                   vw => vw.TaskOption.IsChecked)
                   .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.RuntimeUnaryFacade.Timeout,
                    vw => vw.TimeoutTextBox.Text)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                   vm => vm.RuntimeUnaryFacade.InternalDelay,
                   vw => vw.InternalTextBox.Text)
                   .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.RuntimeUnaryFacade.ServerDelay,
                    vw => vw.ServerDelayTextBox.Text)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                      vm => vm.RuntimeUnaryFacade.IsUseMutiClient,
                      vw => vw.RuntimeTestMultiClientCheckBox.IsChecked)
                      .DisposeWith(dispos);

                this.Bind(ViewModel,
                      vm => vm.RuntimeUnaryFacade.MinTimeElapsed,
                      vw => vw.RuntimeTestTroughCost.Content)
                      .DisposeWith(dispos);

                this.Bind(ViewModel,
                     vm => vm.RuntimeUnaryFacade.MaxTimeElapsed,
                     vw => vw.RuntimeTestPeakCost.Content)
                     .DisposeWith(dispos);

                this.Bind(ViewModel,
                     vm => vm.RuntimeUnaryFacade.AverageTimeElapsed,
                     vw => vw.RuntimeTestAverageCost.Content)
                     .DisposeWith(dispos);

                this.Bind(ViewModel,
                    vm => vm.RuntimeUnaryFacade.MaxCount,
                    vw => vw.RuntimeTestMaxCount.Content)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel,
                    vm => vm.RuntimeUnaryFacade.CostSeries,
                    vw => vw.RuntimeTestCostChart.Series)
                    .DisposeWith(dispos);

                this.OneWayBind(ViewModel,
                    vm => vm.RuntimeUnaryFacade.ChartSyncContext,
                    vw => vw.RuntimeTestCostChart.SyncContext)
                    .DisposeWith(dispos);

                this.Bind(ViewModel,
                   vm => vm.RuntimeUnaryFacade.AllCount,
                   vw => vw.RuntimeTestAllCount.Content)
                   .DisposeWith(dispos);

                this.OneWayBind(ViewModel,
                    vm => vm.RuntimeUnaryFacade.UnaryTracks,
                    vw => vw.RuntimeTestUnaryTracks.ItemsSource)
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                    vm => vm.RuntimeUnaryOnceCommand,
                    v => v.RuntimeTestBeepUnaryButton,
                    vm => vm.UnaryMessage)
                    .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                   vm => vm.RuntimeUnaryFacade.StartAutoUnaryCommand,
                   v => v.RuntimeTestStartAutoUnary,
                   vm => vm.UnaryMessage,
                   nameof(RuntimeTestStartAutoUnary.Checked))
                   .DisposeWith(dispos);

                this.BindCommand(ViewModel,
                    vm => vm.RuntimeUnaryFacade.StopAutoUnaryCommand,
                    v => v.RuntimeTestStartAutoUnary,
                    nameof(RuntimeTestStartAutoUnary.Unchecked))
                    .DisposeWith(dispos);

                #endregion

            });

            this.WhenAnyValue(x => x.ViewModel.IsChecked)
            .Subscribe(newValue =>
            {
                Console.WriteLine($"IsChecked changed to: {newValue}");
            });






        }

        private void GroupBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/dotnet/wpf/issues/4362");
        }
    }
}
