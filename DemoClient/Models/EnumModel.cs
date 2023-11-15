using System;

namespace DemoClient.Models
{
    public enum GrpcMethodType
    {
        Unary,
        ClientStreaming,
        ServerStreaming,
        DuplexStreaming
    }
    [Flags]
    public enum TrackStatusType
    {
        UnKnown = 0,
        Requesting = 1 << 0,
        Proceeding = 1 << 1,
        Done = 1 << 2,
        Stop = 1 << 3,
        Error = 1 << 4,
        Closed = 1 << 5,
    }
}
