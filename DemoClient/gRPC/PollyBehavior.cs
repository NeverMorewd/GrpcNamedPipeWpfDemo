using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Polly;
using Grpc.Core.Interceptors;

namespace DemoClient.gRPC
{
    public class PollyBehavior : Interceptor
    {
        public PollyBehavior() : base()
        {

        }
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {;
            var timeoutPolicy = Policy.TimeoutAsync(3600);
            var retryPolicy =
            Policy<AsyncUnaryCall<TResponse>>
            .Handle<RpcException>(s =>
             s.StatusCode == StatusCode.Internal
             || s.StatusCode == StatusCode.Unknown
             || s.StatusCode == StatusCode.Unavailable)
            .Or<WebException>()
            .Or<SocketException>()
            .OrResult(r =>
            {
                var awaiter = r.GetAwaiter();
                if (awaiter.IsCompleted)
                    return r.ResponseAsync == null;
                try
                {
                    r.ResponseAsync.Wait();
                }
                catch (Exception) { return true; }
                return false;
            })
            .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(1), (result, timeSpan, current, contextPolly) =>
            {
                if (result.Exception != null)
                {
                    //
                }
                else
                {
                    //
                }
            });
            var continuationResult = retryPolicy.WrapAsync(timeoutPolicy).ExecuteAsync(() => Task.FromResult(continuation(request, context)));
            return continuationResult.Result;
        }
    }
}
