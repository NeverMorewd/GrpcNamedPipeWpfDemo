using DemoServer.MethodBoundary;
using Grpc.Core;
using MethodTimer;
using Rpa.Core.Beep;

namespace DemoServer
{
    public class BeepGrpcService : BeepService.BeepServiceBase
    {
        private readonly int pid = Environment.ProcessId;
        private readonly string osVersion = Environment.OSVersion.ToString();


        [UnaryLog]
        [Time("Beep content: '{request}'")]
        public override async Task<Beep> BeepUnary(Beep request, ServerCallContext context)
        {
            request.Payload.Content = $"Server-{pid}-{osVersion}-To-{context.Peer}";
            if (request.Payload.Delay > 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(request.Payload.Delay));
            }
            return await Task.FromResult(request); 
        }

        public override async Task<Beep> BeepStreamingFromClient(IAsyncStreamReader<Beep> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext(context.CancellationToken).ConfigureAwait(false))
            {
                Console.WriteLine($"BeepStreamingFromClient:{requestStream.Current.Payload.Content} - {context.Peer}");
                await Task.Delay(100);
            }
            return await Task.FromResult(new Beep { Payload = new Payload { Content = $"{context.Peer}:BeepStreamingFromClient Over!" } });
        }

        public override async Task BeepStreamingFromServer(Beep request, IServerStreamWriter<Beep> responseStream, ServerCallContext context)
        {
            int streamCount = 0;
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var response = CreateResponse(request, $"I am a serverstreaming message! - {context.Peer}", streamCount++);
                await responseStream.WriteAsync(response);
                Console.WriteLine($"BeepStreamingFromServer: {response}");
                if (request.Payload.Delay > 0)
                {
                    await Task.Delay(request.Payload.Delay);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }
        }

        private Beep CreateResponse(Beep request,string peerString = "", int StreamCount = 1)
        {
            return new Beep
            {
                Payload = new Payload
                {
                    ClientId = request.Payload.ClientId,
                    Content = $"{pid}-{DateTime.Now.ToUniversalTime()}-{StreamCount}-{peerString}-{StreamCount}",
                }
            };
        }
    }
}
