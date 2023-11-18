using Grpc.Core;
using Grpc.Core.Interceptors;
using GrpcDotNetNamedPipes;
using MethodTimer;
using Rpa.Core.Beep;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoClient.gRPC
{
    public class BeepServiceProvider
    {
        private readonly BeepService.BeepServiceClient _beepService;
        public static readonly string ClientTag = $"{Environment.ProcessId}-{Environment.CurrentManagedThreadId}";
        private readonly CallInvoker _channel;
        private readonly string _channelName;
        private readonly string _clientInfo;

        public BeepServiceProvider(string aChannelName,[CallerMemberName]string aClientInfo = "")
        {
            if (string.IsNullOrEmpty(aChannelName))
            {
                throw new ArgumentNullException(nameof(aChannelName));
            }
            _channelName = aChannelName;
            _clientInfo = aClientInfo;
            _channel = new NamedPipeChannel(".", _channelName);
            _beepService = new BeepService.BeepServiceClient(_channel);
        }
        public BeepServiceProvider Clone()
        {
            return new BeepServiceProvider(_channelName, _clientInfo);
        }
        public async Task<string> Beep(string aContent,
            TimeSpan timeout,
            int aReturnDelay,
            CancellationToken cancellationToken = default)
        {
            if (timeout > TimeSpan.FromDays(1))
            {
                throw new ArgumentOutOfRangeException("timeout can not exceed 1 day!");
            }
            CallOptions callOptions = 
                        new
                        (   
                            deadline: DateTime.Now.AddMilliseconds(timeout.TotalMilliseconds).ToUniversalTime(),
                            cancellationToken: cancellationToken
                        );
            var request = BuildBeep(aContent, aReturnDelay);
            var response = await BeepUnaryInternal(request, callOptions);
            return response.Payload.Content;
        }

        public async Task<string> Beep(string aContent,
            int aReturnDelay = -1,
            CancellationToken cancellationToken = default)
        {
            TimeSpan deadLine = aReturnDelay > 0 ? TimeSpan.FromMilliseconds(aReturnDelay) : TimeSpan.FromDays(1);
            return await Beep(aContent, deadLine, aReturnDelay, cancellationToken);
        }


        [Time]
        public AsyncClientStreamingCall<Beep,Beep> BeepClientStreaming(CallOptions callOptions)
        {
            return _beepService.BeepStreamingFromClient(callOptions);
        }

        [Time]
        public AsyncServerStreamingCall<Beep> BeepServerStreaming(Beep beep,CallOptions callOptions)
        {
            return _beepService.BeepStreamingFromServer(beep,callOptions);
        }


        [Time]
        public AsyncServerStreamingCall<Beep> RequestFromServerStreaming(Beep beep, CallOptions callOptions)
        {
            return _beepService.RequestStreamingFromServer(beep, callOptions);
        }

        [Time]
        public AsyncClientStreamingCall<Beep, Beep> ResponseFromClientStreaming(CallOptions callOptions)
        {
            return _beepService.ResponseStreamingFromClient(callOptions);
        }


        [Time]
        private async Task<Beep> BeepUnaryInternal(Beep request,
            CallOptions callOptions)
        {
            return await _beepService.BeepUnaryAsync(request, callOptions);
        }

        private static Beep BuildBeep(string aContent, int aReturnDelay)
        {
            return new Beep
            {
                Payload = new Payload
                {
                    Content = aContent,
                    ClientId = ClientTag,
                    Delay = aReturnDelay
                }
            };
        }  

        public static string GetServiceName()
        {
            return nameof(BeepService.BeepServiceClient);
        }
    }
}
