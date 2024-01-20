using Grpc.Core;
using GrpcDotNetNamedPipes;
using Rpa.Core.Runtimehost;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using MethodTimer;
using Nevermore.Grpc.Ipc;
using DemoClient.Common;

namespace DemoClient.gRPC
{
    public class RuntimeServiceProvider : BaseServiceProvider<RunTimeService.RunTimeServiceClient>
    {
        private readonly RunTimeService.RunTimeServiceClient _runTimeServiceClient;
        private readonly CallInvoker _channel;
        private readonly string _channelName;
        private readonly string _clientInfo;

        public RuntimeServiceProvider(string aChannelName, [CallerMemberName] string aClientInfo = "")
        {
            if (string.IsNullOrEmpty(aChannelName))
            {
                throw new ArgumentNullException(nameof(aChannelName));
            }
            _channelName = aChannelName;
            _clientInfo = aClientInfo;
            _channel = new NamedPipeChannel(".", _channelName);
            _runTimeServiceClient = new RunTimeService.RunTimeServiceClient(_channel);
        }
        public override IBeepServiceProvider Clone()
        {
            return new RuntimeServiceProvider(_channelName, _clientInfo);
        }
        public override async Task<string> Beep(string aContent,
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

            var request = new BeepMessage
            {
                ClientId = _clientInfo,
                Payload = aContent,
                ProcessId = Environment.ProcessId,
            };
            var response = await BeepUnaryInternal(request, callOptions);
            return response.Payload;
        }

        [Time]
        private async Task<BeepMessage> BeepUnaryInternal(BeepMessage request,
            CallOptions callOptions)
        {
            return await _runTimeServiceClient.BeepUnaryAsync(request, callOptions);
        }
    }
}
