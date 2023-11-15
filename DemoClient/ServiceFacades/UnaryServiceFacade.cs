using DemoClient.Common;
using DemoClient.gRPC;
using DemoClient.Models;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace DemoClient.ServiceFacades
{
    /// <summary>
    /// Facade for unary service only
    /// </summary>
    public class UnaryServiceFacade
    {
        private readonly IDisposable? _cleanUpAll;
        private readonly IDisposable? _tracksCleanUp;
        private readonly IDisposable? _inputCleanUp;
        private readonly IObservable<string> _inputObservable;
        private IDisposable? _autoInputCleanUp;
        private readonly IObservable<Unit> _clearObservable;
        private readonly ReadOnlyObservableCollection<UnaryTrackModelProxy> _unaryTracks;
        private readonly Subject<UnaryTrackModel> _trackCacheSubject;
        private BeepServiceProvider _beepServiceProvider;
        private readonly IObservableCache<UnaryTrackModel, long> _cacheData;

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="beepServiceProvider">BeepServiceProvider</param>
        /// <param name="inputObservable">input reactive commmand</param>
        /// <param name="clearObservable">clear reactive commmand</param>
        public UnaryServiceFacade(
            BeepServiceProvider beepServiceProvider, 
            IObservable<string> inputObservable,
            IObservable<Unit> clearObservable)
        {
            _trackCacheSubject = new Subject<UnaryTrackModel>();

            _inputObservable = inputObservable;
            _clearObservable = clearObservable;
            _beepServiceProvider = beepServiceProvider;

            ///inputobservable Subscribe input from ReactiveCommand and Convert to _trackCacheSubject
            _inputCleanUp = _inputObservable
               .Select(x => new UnaryTrackModel(DateTime.Now.ToFileTime(),
                       DateTime.Now,
                       Global.Singleton.ChannelName,
                       x,
                       BeepServiceProvider.ClientTag,
                       BeepServiceProvider.GetServiceName()))
               .Subscribe(x =>
               {
                   UnaryTaskExecute(x);
               });

            ///Bind to ui with cache
            var dataConnectable = LoadAndMaintainCache().Publish();
                        var dataCleanUp = dataConnectable.Connect();
            _cacheData = dataConnectable.AsObservableCache();
            _tracksCleanUp = _cacheData
                .Connect()
                .Transform(x => new UnaryTrackModelProxy(x))
                .Sort(SortExpressionComparer<UnaryTrackModelProxy>.Descending(t => t.RequestTime), SortOptimisations.None, 25)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _unaryTracks)
                .DisposeMany()
                .Subscribe();


            StartAutoUnaryCommand = ReactiveCommand.Create<string>((t)
                => 
            {
                _autoInputCleanUp =
                Observable
                .Interval(TimeSpan.FromMilliseconds(InternalDelay))
                .Select(x => new UnaryTrackModel(DateTime.Now.ToFileTime(),
                       DateTime.Now,
                       Global.Singleton.ChannelName,
                       $"{t}-{x}",
                       BeepServiceProvider.ClientTag,
                       BeepServiceProvider.GetServiceName()))
                .Subscribe(x => UnaryTaskExecute(x));
            });

            StopAutoUnaryCommand = ReactiveCommand.Create(()=> _autoInputCleanUp?.Dispose());




            _cleanUpAll = new CompositeDisposable(_tracksCleanUp, _inputCleanUp, dataCleanUp);

        }
        public ReadOnlyObservableCollection<UnaryTrackModelProxy> UnaryTracks => _unaryTracks;

        [Reactive]
        public bool IsLongRunnig
        {
            get;
            set;
        }

        [Reactive]
        public int Timeout
        {
            get;
            set;
        } = 3000;

        [Reactive]
        public int ServerDelay
        {
            get;
            set;
        } = 0;

        [Reactive]
        public int InternalDelay
        {
            get;
            set;
        } = 50;

        [Reactive]
        public bool IsUseMutiClient
        {
            get;
            set;
        } = false;

        public ReactiveCommand<string, Unit> StartAutoUnaryCommand
        {
            get;
            set;
        }
        public ReactiveCommand<Unit, Unit> StopAutoUnaryCommand
        {
            get;
            set;
        }
        /// <summary>
        /// load cache
        /// </summary>
        /// <returns></returns>
        private IObservable<IChangeSet<UnaryTrackModel, long>> LoadAndMaintainCache()
        {
            //construct an cache datasource specifying that the primary key is UnaryTrackModel.Id
            return ObservableChangeSet.Create<UnaryTrackModel, long>(caches =>
            {
                var limitCleaner = caches.LimitSizeTo(100).Subscribe();
                var cacheCleaner = _trackCacheSubject.Subscribe(t => 
                {
                    caches.AddOrUpdate(t); 
                });
                var clearCleaner = _clearObservable.Subscribe(t => 
                {
                    ///remain uncompleted track
                    foreach (var cache in caches.Items)
                    {
                        if ((cache.Status | TrackStatusType.Proceeding) > TrackStatusType.Done)
                        {
                            caches.Remove(cache);
                        }
                        else
                        {
                            Console.WriteLine($"{cache.Id} is in procceding,Ignore clear command!");
                        }
                    }
                    //caches.Clear();
                });

                return new CompositeDisposable(limitCleaner, cacheCleaner, clearCleaner);
            },  track => track.Id);
        }

        public void Dispose()
        {
            _cleanUpAll?.Dispose();
        }

        /// <summary>
        /// UnaryTaskExecute
        /// </summary>
        /// <param name="unaryTrackModel"></param>
        private void UnaryTaskExecute(UnaryTrackModel unaryTrackModel)
        {
            _trackCacheSubject.OnNext(unaryTrackModel);

            var taskCreateOption = IsLongRunnig ? TaskCreationOptions.LongRunning : TaskCreationOptions.None;
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    unaryTrackModel.SetStatus(TrackStatusType.Requesting);

                    if (IsUseMutiClient)
                    {
                        _beepServiceProvider = _beepServiceProvider.Clone();
                    }

                    var responseTask = _beepServiceProvider.Beep(unaryTrackModel.Request, TimeSpan.FromMilliseconds(Timeout), ServerDelay);

                    unaryTrackModel.SetStatus(TrackStatusType.Proceeding);

                    var response = await responseTask;

                    unaryTrackModel.SetResult(TrackStatusType.Done, Grpc.Core.StatusCode.OK, response);
                }
                catch (Grpc.Core.RpcException gex)
                {
                    unaryTrackModel.SetResult(TrackStatusType.Error, gex.StatusCode, gex.Message);
                }
                catch (Exception ex)
                {
                    unaryTrackModel.SetResult(TrackStatusType.Error, Grpc.Core.StatusCode.Aborted, ex.Message);
                }
                finally
                {

                }
            }, taskCreateOption);
        }

    }
}
