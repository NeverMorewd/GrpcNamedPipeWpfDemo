using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DemoClient.Models
{
    public class UnaryTrackModel : IDisposable, IEquatable<UnaryTrackModel>
    {
        private readonly ISubject<UnaryTrackModel> _trackStatusChangedSubject 
            = new  Subject<UnaryTrackModel>();

        public UnaryTrackModel(long anId,
            DateTime aRequestTime,
            string aHost,
            string aRequest,
            string aProto,
            string aServiceMethod)
        {
            Id = anId;
            RequestTime = aRequestTime;
            Request = aRequest;
            Host = aHost;
            Proto = aProto;
            ServiceMethod = aServiceMethod;
            Status = TrackStatusType.UnKnown;
            StatusCode = Grpc.Core.StatusCode.Unknown;
            Response = string.Empty;
        }
        public UnaryTrackModel(UnaryTrackModel aTrack, 
            TrackStatusType aStatus)
        {
            Id = aTrack.Id;
            RequestTime = aTrack.RequestTime;
            Host = aTrack.Host;
            RequestType = aTrack.RequestType;
            Proto = aTrack.Proto;
            Request = aTrack.Request;
            Status = aStatus;
            ServiceMethod = aTrack.ServiceMethod;
            StatusCode = Grpc.Core.StatusCode.Unknown;
            Response = string.Empty;
        }

        public long Id { get; set; }
        public DateTime RequestTime { get; set; }
        public string Host { get; set; }
        public GrpcMethodType RequestType { get; set; }
        public string Request { get; set; }
        public TrackStatusType Status { get; set; }
        public string Response { get; set; }
        public Grpc.Core.StatusCode StatusCode { get; set; }
        public string Proto { get; set; }
        public string ServiceMethod { get; set; }
        public TimeSpan AllElapsed { get; set; }

        public IObservable<UnaryTrackModel> TrackStatusChanged
            => _trackStatusChangedSubject.AsObservable();

        public void SetStatus(TrackStatusType aStatus)
        {
            Status = aStatus;
            if ((aStatus | TrackStatusType.Requesting) >= TrackStatusType.Done)
            {
                AllElapsed = DateTime.Now - RequestTime;
            }
            if (aStatus == TrackStatusType.Error)
            {

            }
            _trackStatusChangedSubject.OnNext(this);
        }

        private void SetStatus(TrackStatusType aStatus,
            Grpc.Core.StatusCode aStatusCode)
        {
            StatusCode = aStatusCode;
            SetStatus(aStatus);
        }

        public void SetResult(TrackStatusType aStatus,
            Grpc.Core.StatusCode aStatusCode,
            string aResult)
        {
            SetResult(aResult);
            SetStatus(aStatus, aStatusCode);
        }

        private void SetResult(string aResult)
        {
            Response = aResult;
        }


        #region equal
        public bool Equals(UnaryTrackModel? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UnaryTrackModel)obj);
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
        public static bool operator ==(UnaryTrackModel left, UnaryTrackModel right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(UnaryTrackModel left, UnaryTrackModel right)
        {
            return !Equals(left, right);
        }
        #endregion

        public void Dispose()
        {
            _trackStatusChangedSubject.OnCompleted();
        }
    }
}
