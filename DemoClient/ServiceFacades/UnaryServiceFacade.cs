using DemoClient.Common;
using DemoClient.gRPC;
using DemoClient.Models;
using DynamicData;
using DynamicData.Binding;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using LiveChartsCore.Themes;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    public class UnaryServiceFacade:ReactiveObject
    {
        private readonly IDisposable? _cleanUpAll;
        private readonly IDisposable? _tracksCleanUp;
        private readonly IObservable<string> _inputObservable;
        private IDisposable? _autoInputCleanUp;
        private readonly IObservable<Unit> _clearObservable;
        private readonly ReadOnlyObservableCollection<UnaryTrackModelProxy> _unaryTracks;
        private readonly IObserver<UnaryTrackModel> _trackCacheObserver;
        private readonly IObservable<UnaryTrackModel> _trackCacheObservable;
        private BeepServiceProvider _beepServiceProvider;
        private readonly IObservableCache<UnaryTrackModel, long> _cacheData;
        private const int IgnoreCost = 200;
        private const int SlowCost = 50;
        private readonly ObservableCollection<ObservableValue> _values;
        private readonly ObservableCollection<ObservableValue> _averageValues;
        private readonly LvcColor[] colors = ColorPalletes.FluentDesign;

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
            _values = new ObservableCollection<ObservableValue>();
            _averageValues = new ObservableCollection<ObservableValue>();
            ChartSyncContext = new object();
            var subject = new ReplaySubject<UnaryTrackModel>();

            _trackCacheObserver = subject.AsObserver();
            _trackCacheObservable = subject.AsObservable();

            _inputObservable = inputObservable;
            _clearObservable = clearObservable;
            _beepServiceProvider = beepServiceProvider;

            var strokeThickness = 2;
            var strokeDashArray = new float[] { 3 * strokeThickness, 2 * strokeThickness };
            var effect = new DashEffect(strokeDashArray);

            var color = colors.First();
            CostSeries = new List<ISeries> 
            {
                new LineSeries<ObservableValue, LiveChartsCore.SkiaSharpView.Drawing.Geometries.RectangleGeometry>
                {
                    Values = _values,
                    Fill = new SolidColorPaint(new SKColor(color.R, color.G, color.B, 90)),
                    LineSmoothness = 1,
                    GeometryFill = new SolidColorPaint(SKColors.Purple),
                    GeometrySize = 4,
                    GeometryStroke = new SolidColorPaint(SKColors.Purple),
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.CornflowerBlue,
                        StrokeCap = SKStrokeCap.Round,
                        StrokeThickness = strokeThickness,
                    },
                },

                new LineSeries<ObservableValue, LiveChartsCore.SkiaSharpView.Drawing.Geometries.CircleGeometry>
                {
                    Values = _averageValues,

                    Fill = null,
                    GeometryStroke = null,
                    GeometryFill = new SolidColorPaint(SKColors.DarkOliveGreen),
                    GeometrySize = 4,
                    Stroke = new SolidColorPaint
                    {
                        Color = SKColors.DarkOliveGreen,
                        StrokeCap = SKStrokeCap.Round,
                        StrokeThickness = strokeThickness,
                    },
                }
            };

            ///Bind to ui with cache
            var dataConnectable = LoadAndMaintainCache().Publish();
            var dataCleanUp = dataConnectable.Connect();
            _cacheData = dataConnectable.AsObservableCache();

            var tracksCleanUp = _cacheData
                .Connect()
                .LimitSizeTo(20000)
                .Transform(x => new UnaryTrackModelProxy(x))
                .Sort(SortExpressionComparer<UnaryTrackModelProxy>.Descending(t => t.RequestTime), SortOptimisations.None, 25)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _unaryTracks)
                .DisposeMany()
                .Subscribe();

            var inputCleanUp = _inputObservable
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

            var minCleaner =  _trackCacheObservable
                .WhereNotNull()
                .Select(t => t.AllElapsed.TotalMilliseconds)
                .Where(t => t > 0 && t < IgnoreCost) //ignore 0 and big number from cold start
                .Scan((min, next) => Math.Min(min, next))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t =>
                {
                    MinTimeElapsed = t;
                });

            var maxCleaner = _trackCacheObservable
                .WhereNotNull()
                .Select(t => t.AllElapsed.TotalMilliseconds)
                .Where(t => t > 0 && t < IgnoreCost) //ignore 0 and big number from cold start
                .Scan((min, next) => Math.Max(min, next))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t =>
                {
                    MaxTimeElapsed = t;
                });

            int subscribeCount = 1;

            var averageCleaner = _trackCacheObservable
                .WhereNotNull()
                .Select(t => t.AllElapsed.TotalMilliseconds)
                .Where(t => t > 0 && t < IgnoreCost)
                .Scan(new { Sum = 0.0, Count = 0 }, (acc, next) => new { Sum = acc.Sum + next, Count = acc.Count + 1 })
                .Where(acc => acc.Count > 0)
                .Select(acc => acc.Sum / acc.Count)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t =>
                {
                    AverageTimeElapsed = t;
                                      
                });

            var maxCountCleaner = _trackCacheObservable
                .WhereNotNull()
                .Select(t => t.AllElapsed.TotalMilliseconds)
                .Where(t => t > SlowCost) 
                .Scan(0, (count, _) => count + 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t =>
                {
                    MaxCount = t;
                });

            var allCountCleaner = _trackCacheObservable
                .WhereNotNull()
                .DistinctUntilChanged()
                .Scan(0, (count, _) => count + 1)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t =>
                {
                    AllCount = t;
                });


            var chartCleaner = _trackCacheObservable
                .WhereNotNull()
                .Select(t => t.AllElapsed.TotalMilliseconds)
                .Where(t=>t>0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t =>
                {
                    lock (ChartSyncContext)
                    {
                        subscribeCount++;
                        _values.Add(new ObservableValue(t));
                        _averageValues.Add(new ObservableValue(AverageTimeElapsed));

                        if (_values.Count > 20000)
                        {
                            _values.RemoveAt(0);
                            _averageValues.RemoveAt(0);
                        }
                    }
                });

            StartAutoUnaryCommand = ReactiveCommand.Create<string>(
                (t)
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


            _cleanUpAll = new CompositeDisposable(
                tracksCleanUp,
                inputCleanUp, 
                dataCleanUp, 
                averageCleaner, 
                maxCleaner, 
                minCleaner,
                maxCountCleaner,
                allCountCleaner,
                chartCleaner);

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
        } = 200;

        [Reactive]
        public double MinTimeElapsed
        {
            get;
            set;
        } = TimeSpan.FromMilliseconds(0).TotalMilliseconds;

        [Reactive]
        public double MaxTimeElapsed
        {
            get;
            set;
        } = TimeSpan.FromMilliseconds(0).TotalMilliseconds;

        [Reactive]
        public int MaxCount
        {
            get;
            set;
        } = 0;

        [Reactive]
        public int AllCount
        {
            get;
            set;
        } = 0;

        [Reactive]
        public double AverageTimeElapsed
        {
            get;
            set;
        } = TimeSpan.FromMilliseconds(0).TotalMilliseconds;

        [Reactive]
        public bool IsUseMutiClient
        {
            get;
            set;
        } = false;

        [Reactive]
        public List<ISeries> CostSeries
        {
            get;
            set;
        }
        public object ChartSyncContext { get; }
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
                var cacheCleaner = _trackCacheObservable.Subscribe(t => 
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
                    //MaxTimeElapsed = 0;
                    //MinTimeElapsed = 0;
                    //AverageTimeElapsed = 0;
                });

                return new CompositeDisposable(cacheCleaner, clearCleaner);
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
            _trackCacheObserver.OnNext(unaryTrackModel);

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
                    _trackCacheObserver.OnNext(unaryTrackModel);
                }
            }, taskCreateOption);
        }

    }
}
