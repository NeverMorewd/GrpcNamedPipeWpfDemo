using DemoClient.gRPC;
using DemoClient.Models;
using DynamicData;
using DynamicData.Binding;
using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Nevermore.Grpc.Ipc;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoClient.ServiceFacades
{
    public class ServerStreamingServiceFacade : ReactiveObject, IDisposable
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private AsyncServerStreamingCall<Beep>? serverStreamCall;
        private readonly IDisposable? _outputObservableCleanup;
        private IObservable<string>? _outputObservable;
        private readonly BeepServiceProvider _beepServiceProvider;
        private readonly SourceList<string> _items = new();
        private readonly ReadOnlyObservableCollection<string> _tracks;
        private bool _isReady = false;
        private readonly IDisposable? _cleanUpAll;
        public ServerStreamingServiceFacade(BeepServiceProvider beepServiceProvider,
            IObservable<string> readyObservable,
            IObservable<string> suspendObservable,
            IObservable<Unit> clearObservable)
        {
            _beepServiceProvider = beepServiceProvider;

            var responseCleanUp = ResponseConnect()
              .Transform(x => x)
              .Filter(x => x != null)
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _tracks)
              .Subscribe();

            var limitCleanUp = _items.LimitSizeTo(100).Subscribe();
            var readyCleanUp = readyObservable.Subscribe(msg => SetReady(msg));
            var suspendCleanUp = suspendObservable.Subscribe(msg => SetSuspend(msg));
            var clearCleanUp = clearObservable.Subscribe(_ => Clear());

            _cleanUpAll = new CompositeDisposable(responseCleanUp, readyCleanUp, suspendCleanUp, clearCleanUp, limitCleanUp);
        }
        [Reactive]
        public int ServerDelay
        {
            get;
            set;
        } = 10;
        public ReadOnlyObservableCollection<string> ResponseTracks => _tracks;
        private IObservable<IChangeSet<string>> ResponseConnect()
        {
            return _items.Connect();
        }
        private void SetReady(string message)
        {
            Console.WriteLine(message);
            Task.Factory.StartNew(() => 
            {
                if (!_isReady)
                {
                    try
                    {
                        var request = new Beep
                        {
                            Payload = new Payload
                            {
                                Content = message,
                                ClientId = $"{BeepServiceProvider.ClientTag}",
                                Delay = ServerDelay,
                            }
                        };

                        _outputObservable =
                        Observable.Create<string>(async (observer) =>
                        {
                            try
                            {
                                _cancellationTokenSource = new CancellationTokenSource();
                                CallOptions callOptions = new(cancellationToken:_cancellationTokenSource.Token);
                                serverStreamCall = _beepServiceProvider.BeepServerStreaming(request, callOptions);
                                var responseStream = serverStreamCall.ResponseStream;
                                while (await responseStream.MoveNext(_cancellationTokenSource.Token).ConfigureAwait(false))
                                {
                                    var response = responseStream.Current;
                                    observer.OnNext(response.Payload.Content);
                                }
                                observer.OnCompleted();
                            }
                            catch (Exception ex)
                            {
                                observer.OnError(ex);
                            }
                        });
                        _outputObservable?.Subscribe(response => _items.Add(response));
                        _isReady = true;
                    }
                    catch (Exception ex)
                    {
                        _items.Add(ex.Message);
                    }
                }
            });           
        }
        private void SetSuspend(string message)
        {
            Console.WriteLine(message);
            if (_isReady)
            {
                _cancellationTokenSource?.Cancel();
                serverStreamCall?.Dispose();
                _outputObservableCleanup?.Dispose();
                _isReady = false;
            }
        }
        private void Clear()
        {
            _items?.Clear();
        }

        public void Dispose()
        {
            _outputObservableCleanup?.Dispose();
            _cleanUpAll?.Dispose();
        }

    }
}
