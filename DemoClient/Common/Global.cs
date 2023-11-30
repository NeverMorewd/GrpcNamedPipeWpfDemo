using DynamicData;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;

namespace DemoClient.Common
{
    public class Global : ReactiveObject,IDisposable
    {
        public const string _DefaultChannelName = "53189515-EE63-427A-870D-F3D11BD96F36";
        private readonly IDisposable _limitCleaner;
        private readonly IDisposable _outPutCleaner;
        private readonly SourceList<string> _outPuts;
        private readonly ReadOnlyObservableCollection<string> _consoleOutPuts;
        private Global()
        {
            ChannelName = _DefaultChannelName;
            _outPuts = new SourceList<string>();

            _limitCleaner = _outPuts.LimitSizeTo(100)
                                    .Subscribe();

            _outPutCleaner = _outPuts.Connect()
                                     .ObserveOn(RxApp.MainThreadScheduler)
                                     .Bind(out _consoleOutPuts)
                                     .Subscribe();
        }

        public static readonly Global Singleton = new();

        public ReadOnlyObservableCollection<string> ConsoleOutPuts => _consoleOutPuts;

        public string ChannelName
        {
            get;
            set;
        }

        public void SetConsoleObservable(IObservable<string?> observable)
        {
          var outputCleaner =  observable.WhereNotNull()
                                         .Subscribe(o => _outPuts.Add(o));
        }

        public void Dispose()
        {
            _limitCleaner?.Dispose();
            _outPutCleaner?.Dispose();
        }
    }
}
