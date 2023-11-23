using DynamicData.Binding;
using Grpc.Core;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DemoClient.Models
{
    public class UnaryTrackModelProxy : ReactiveObject,
        IDisposable,
        IEquatable<UnaryTrackModelProxy>
    {
        private readonly UnaryTrackModel _track;
        private readonly IDisposable _cleanUp;
        private readonly long _id;
        public UnaryTrackModelProxy(UnaryTrackModel aTrack)
        {
            _id = aTrack.Id;
            _track = aTrack;
            Status = _track.Status;
            StatusCode = _track.StatusCode;
            AllElapsed = _track.AllElapsed;
            Response = _track.Response;

            var isRecent = DateTime.Now.Subtract(aTrack.RequestTime).TotalSeconds < 2;
            var recentIndicator = Disposable.Empty;

            if (isRecent)
            {
                Recent = true;
                recentIndicator = Observable.Timer(TimeSpan.FromSeconds(2))
                    .Subscribe(_ => Recent = false);
            }
            var statusRefresher = aTrack.TrackStatusChanged
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(t =>
                {
                    AllElapsed = t.AllElapsed;
                    StatusCode = t.StatusCode;
                    Status = t.Status;
                    Response = t.Response;
                });

            _cleanUp = Disposable.Create(() =>
            {
                recentIndicator.Dispose();
                statusRefresher.Dispose();
            });
        }
        public bool Recent
        {
            get;
            set;
        }
        [Reactive]
        public TrackStatusType Status
        {
            get;
            set;
        }
        [Reactive]
        public StatusCode StatusCode
        {
            get;
            set;
        }
        [Reactive]
        public string? Response
        {
            get;
            set;
        }
        [Reactive]
        public TimeSpan AllElapsed
        {
            get;
            set;
        }
        public long Id => _id;
        public DateTime RequestTime => _track.RequestTime;
        public string Host => _track.Host;
        public GrpcMethodType RequestType => _track.RequestType;
        public string Request => _track.Request;
        public string Proto => _track.Proto;
        public string Discription => _track.ServiceMethod;

        #region Equaility Members

        public bool Equals(UnaryTrackModelProxy? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id == other._id;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UnaryTrackModelProxy)obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool operator ==(UnaryTrackModelProxy left, UnaryTrackModelProxy right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(UnaryTrackModelProxy left, UnaryTrackModelProxy right)
        {
            return !Equals(left, right);
        }

        #endregion

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
