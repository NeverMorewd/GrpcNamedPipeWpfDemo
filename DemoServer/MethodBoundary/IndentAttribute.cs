using MethodBoundaryAspect.Fody.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoServer.MethodBoundary
{
    public sealed class IndentAttribute : OnMethodBoundaryAspect
    {
        public override void OnExit(MethodExecutionArgs args)
        {
            args.ReturnValue = String.Concat(args.ReturnValue.ToString().Split('\n').Select(line => "  " + line));
        }
    }
}
