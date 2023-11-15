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
                  vw => vw.MutiClientCheckBox.IsChecked)
                  .DisposeWith(dispos);

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
