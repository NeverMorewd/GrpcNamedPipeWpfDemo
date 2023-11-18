using DemoClient.gRPC;
using DynamicData;
using Grpc.Core;
using ReactiveUI;
using Nevermore.Grpc.Ipc;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoClient.ServiceFacades
{
    public class ClientStreamingServiceFacade : ReactiveObject, IDisposable
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private AsyncClientStreamingCall<Beep, Beep>? clientStreamCall;
        private IDisposable? _inputObservableCleanup;
        private bool _isReady = false;
        private readonly IDisposable? _cleanUpAll;
        private readonly BeepServiceProvider _beepServiceProvider;
        private readonly ReadOnlyObservableCollection<string> _tracks;
        private readonly SourceList<string> _items = new();
        private readonly IObservable<string>? _inputObservable;

        public ClientStreamingServiceFacade(BeepServiceProvider beepServiceProvider,
            IObservable<string> inputObservable,
            IObservable<string> readyObservable,
            IObservable<string> suspendObservable,
            IObservable<Unit> clearObservable)
        {
            _beepServiceProvider = beepServiceProvider;
            _inputObservable = inputObservable;

            var readyCleanUp = readyObservable.Subscribe(msg => SetReady(msg));
            var suspendCleanUp = suspendObservable.Subscribe(msg => SetSuspend(msg));
            var clearCleanUp = clearObservable.Subscribe(_ => Clear());

            var limitCleanUp = _items.LimitSizeTo(1000).Subscribe();

            var cleanUp = this.TracksConnect()
               .Transform(x => x)
               .Filter(x => x != null)
               .ObserveOn(RxApp.MainThreadScheduler)
               .Bind(out _tracks)
               .Subscribe();

            _cleanUpAll = new CompositeDisposable(cleanUp, readyCleanUp, suspendCleanUp, clearCleanUp, limitCleanUp);
        }

        public ReadOnlyObservableCollection<string> Tracks => _tracks;

        private IObservable<IChangeSet<string>> TracksConnect()
        {
            return _items.Connect();
        }

        private void SetReady(string message)
        {
            Console.WriteLine(message);
            if (!_isReady)
            {
                Execute();
                _isReady = true;
            }
        }
        private void SetSuspend(string message)
        {
            Console.WriteLine(message);
            if (_isReady)
            {
                _cancellationTokenSource?.Cancel();
                clientStreamCall?.Dispose();
                _inputObservableCleanup?.Dispose();
                _isReady = false;
            }
        }
        private void Execute()
        {
            Task.Factory.StartNew(() =>
            {
                _cancellationTokenSource = new CancellationTokenSource();
                CallOptions callOptions = new(cancellationToken: _cancellationTokenSource.Token);
                try
                {
                    clientStreamCall = _beepServiceProvider.BeepClientStreaming(callOptions);

                    _inputObservableCleanup = _inputObservable?
                            .Select(s => new Beep
                            {
                                Payload = new Payload
                                {
                                    Content = s,
                                    ClientId = $"ClientStreamingService"
                                }
                            })
                            .Subscribe
                            (
                                async b =>
                                {
                                    if (_isReady)
                                    {
                                        await clientStreamCall.RequestStream.WriteAsync(b);
                                        _items?.Add(b.Payload.Content);
                                    }
                                },
                                async ex =>
                                {
                                    await clientStreamCall.RequestStream.CompleteAsync();
                                },
                                async () =>
                                {
                                    await clientStreamCall.RequestStream.CompleteAsync();
                                }
                            );
                }
                catch (Exception ex)
                {
                    _items?.Add(ex.Message);
                }
            });
        }
        public void Clear()
        {
            _items?.Clear();
        }
        public void Dispose()
        {
            _inputObservableCleanup?.Dispose();
            _cleanUpAll?.Dispose();
        }

    }
}
