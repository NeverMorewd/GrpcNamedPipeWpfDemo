using DemoClient.Common;
using DemoClient.gRPC;
using DemoClient.ServiceFacades;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace DemoClient
{
    public class AppViewModel: ReactiveObject,IDisposable
    {
        private readonly IDisposable? _disposable;
        private readonly BeepServiceProvider _beepProvider;

        public AppViewModel()
        {
            _beepProvider = new BeepServiceProvider(Global.Singleton.ChannelName);

            #region unary
            UnaryOnceCommand = ReactiveCommand.CreateFromTask<string, string>(message => Task.FromResult(message));
            UnaryClearCommand = ReactiveCommand.Create(() =>
            {
                Console.WriteLine(nameof(UnaryClearCommand));
            });
            UnaryFacade = new UnaryServiceFacade(_beepProvider, UnaryOnceCommand, UnaryClearCommand);
            #endregion

            #region ServerStreaming
            StartServerStreamingCommand = ReactiveCommand.CreateFromTask<string, string>(message =>
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = nameof(StartServerStreamingCommand);
                }
                return Task.FromResult(message);
            });
            StopServerStreamingCommand = ReactiveCommand.CreateFromTask(() => Task.FromResult(nameof(StopServerStreamingCommand)));
            ServerStreamingClearCommand = ReactiveCommand.CreateFromTask(() => Task.Delay(0));
            ServerStreamingFacade = new ServerStreamingServiceFacade(_beepProvider,
                StartServerStreamingCommand,
                StopServerStreamingCommand,
                ServerStreamingClearCommand);
            #endregion

            #region ClientStreaming
            PushOnceCommand = ReactiveCommand.Create<string, string>(p => $"{DateTime.Now.ToLongTimeString()}:{p}");
            StartClientStreamingCommand = ReactiveCommand.CreateFromTask(() => Task.FromResult(nameof(StartClientStreamingCommand)));
            StopClientStreamingCommand = ReactiveCommand.CreateFromTask(() => Task.FromResult(nameof(StopClientStreamingCommand)));
            ClientStreamingClearCommand = ReactiveCommand.CreateFromTask(() => Task.Delay(0));

            ClientStreamingFacade = new ClientStreamingServiceFacade(_beepProvider,
                PushOnceCommand,
                StartClientStreamingCommand,
                StopClientStreamingCommand,
                ClientStreamingClearCommand);
            #endregion

            #region ConverseStreaming

            StartConverseStreamingCommand = ReactiveCommand.CreateFromTask<string, string>(message =>
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = nameof(StartConverseStreamingCommand);
                }
                return Task.FromResult(message);
            });
            StopConverseStreamingCommand = ReactiveCommand.CreateFromTask(() => Task.FromResult(nameof(StopConverseStreamingCommand)));
            ConverseStreamingClearCommand = ReactiveCommand.CreateFromTask(() => Task.Delay(0));
            ConverseStreamingFacade = new ConverseStreamingServiceFacade(_beepProvider,
                StartConverseStreamingCommand,
                StopConverseStreamingCommand,
                ConverseStreamingClearCommand);
        }

            #endregion


        public UnaryServiceFacade UnaryFacade 
        { 
            get; 
            private set; 
        }

        public UnaryServiceFacade RuntimeUnaryFacade
        {
            get;
            private set;
        }
        public ClientStreamingServiceFacade ClientStreamingFacade 
        { 
            get; 
            private set; 
        }

        public ServerStreamingServiceFacade ServerStreamingFacade
        {
            get;
            private set;
        }

        public ConverseStreamingServiceFacade ConverseStreamingFacade
        {
            get;
            private set;
        }

        [Reactive]
        public bool IsChecked
        {
            get;
            set;
        }

        [Reactive]
        public string ClientMessage
        {
            get;
            set;
        } = "I am a clientstreaming message!";

        [Reactive]
        public string UnaryMessage
        {
            get;
            set;
        } = "I am an unary message!";

        private bool _isCheckedInternal;
        public bool IsCheckedInternal
        {
            get => _isCheckedInternal;
            set
            {
                _isCheckedInternal = value;
                IsChecked = _isCheckedInternal;
            }
        }
        public ReactiveCommand<string, string> UnaryOnceCommand
        {
            get;
            set;
        }
        public ReactiveCommand<string, string> PushOnceCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, string> StartClientStreamingCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, string> StopClientStreamingCommand
        {
            get;
            set;
        }
        public ReactiveCommand<string, string> StartServerStreamingCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, string> StopServerStreamingCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, Unit> UnaryClearCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, Unit> ClientStreamingClearCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, Unit> ServerStreamingClearCommand
        {
            get;
            set;
        }
        public ReactiveCommand<string, string> StartConverseStreamingCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, string> StopConverseStreamingCommand
        {
            get;
            set;
        }

        public ReactiveCommand<Unit, Unit> ConverseStreamingClearCommand
        {
            get;
            set;
        }
        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
