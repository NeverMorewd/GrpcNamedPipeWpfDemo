using Grpc.Core;
using Nevermore.Grpc.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoClient.gRPC
{
    public abstract class BaseServiceProvider<T> : IBeepServiceProvider where T : ClientBase
    {
        public static readonly string ClientTag = $"{Environment.ProcessId}-{Environment.CurrentManagedThreadId}";

        public abstract Task<string> Beep(string aContent,
           TimeSpan timeout,
           int aReturnDelay,
           CancellationToken cancellationToken = default);

        public string GetServiceName()
        {
            return nameof(T);
        }

        public abstract IBeepServiceProvider Clone();
    }

    public interface IBeepServiceProvider
    {
        Task<string> Beep(string aContent,
           TimeSpan timeout,
           int aReturnDelay,
           CancellationToken cancellationToken = default);

        string GetServiceName();

        IBeepServiceProvider Clone();
    }
}
