using MethodBoundaryAspect.Fody.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoServer.MethodBoundary
{
    public sealed class HandleTaskExceptionAttribute : OnMethodBoundaryAspect
    {
        public override void OnExit(MethodExecutionArgs args)
        {
            if (args.ReturnValue is Task<string> task)
            {
                args.ReturnValue = task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        return "An error happened: " + t.Exception.Message;
                    return t.Result;
                });
            }
        }
    }
}
