#if NET6_0_OR_GREATER
using System.Net.Http;
using Grpc.Net.Client;

namespace GrpcDotNetNamedPipes.PerfTests.Helpers
{
    public class OriginalHttpConextFactory:ChannelContextFactory
    {
        private int _port;
        public override ChannelContext Create(ITestOutputHelper output = null)
        {
            _port = 9902;
            var channelOptions = new List<ChannelOption>
            {
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue)
            };

            var server = new Server(channelOptions);
            server.Ports.Add("127.0.0.1", _port, ServerCredentials.Insecure);
            server.Services.Add(TestService.BindService(new TestServiceImpl()));
            server.Start();

            return new ChannelContext
            {
                Impl = new TestServiceImpl(), // TODO: Match instance
                Client = CreateClient(output),
                OnDispose = () => server.KillAsync()
            };
        }
        public override ChannelContext WarmCreate(ITestOutputHelper output = null, int port = 0)
        {
            _port = port;
            var channelOptions = new List<ChannelOption>
            {
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue)
            };

            var server = new Server(channelOptions);
            server.Ports.Add("127.0.0.1", port, ServerCredentials.Insecure);
            server.Services.Add(TestService.BindService(new TestServiceImpl()));
            server.Start();

            return new ChannelContext
            {
                Impl = new TestServiceImpl(), // TODO: Match instance
                Client = CreateClient(output),
                OnDispose = () => server.KillAsync()
            };
        }
        public override TestService.TestServiceClient CreateClient(ITestOutputHelper output = null)
        {
            //var httpHandler = new HttpClientHandler();
            //httpHandler.ServerCertificateCustomValidationCallback =
            //    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            return new TestService.TestServiceClient(GrpcChannel.ForAddress($"http://127.0.0.1:{_port}", new GrpcChannelOptions { MaxReceiveMessageSize = int.MaxValue ,MaxSendMessageSize = int.MaxValue}));
        }
    }
}
#endif