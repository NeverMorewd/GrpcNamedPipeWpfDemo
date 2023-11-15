using Google.Protobuf;
using Grpc.Core;
using MethodBoundaryAspect.Fody.Attributes;
using Rpa.Core.Beep;
using System.Diagnostics;
using System.Text.Json;

namespace DemoServer.MethodBoundary
{
    public sealed class UnaryLogAttribute : OnMethodBoundaryAspect
    {
        public UnaryLogAttribute() 
        {

        }
        public override void OnEntry(MethodExecutionArgs args)
        {
            Console.WriteLine();
            Console.WriteLine($"==================== OnEntry {args.Method.Name}-{DateTime.Now:MM/dd/yyyy HH:mm:ss.fff} ====================");
            if (args.Arguments != null && args.Arguments.Any())
            {
                if (args.Arguments[1] is ServerCallContext context)
                {
                    var peer = JsonSerializer.Serialize(context.Peer,new JsonSerializerOptions { IncludeFields = false });
                    var header = JsonSerializer.Serialize(context.RequestHeaders, new JsonSerializerOptions { IncludeFields = false });
                    Console.WriteLine($"# peer:{peer}");
                    Console.WriteLine($"# header:{header}");
                }
                if (args.Arguments[0] is Beep beep)
                {
                    var beepJson = JsonFormatter.Default.Format(beep);
                    Console.WriteLine($"# message:{beepJson}");
                }
            }
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            if (args.ReturnValue is Task t)
            {
                t.ContinueWith(task =>
                {
                    Console.WriteLine($"==================== OnExit {args.Method.Name}-{DateTime.Now:MM/dd/yyyy HH:mm:ss.fff} ====================");
                });
            }
            else
            {
                Console.WriteLine($"==================== OnExit {args.Method.Name}-{DateTime.Now:MM/dd/yyyy HH:mm:ss.fff} ====================");
            }
        }

        public override void OnException(MethodExecutionArgs args)
        {
            Console.WriteLine($"==================== OnException {args.Method.Name}-{DateTime.Now:MM/dd/yyyy HH:mm:ss.fff} ====================");
            Console.WriteLine($"{args.Exception}");
        }
    }
}
