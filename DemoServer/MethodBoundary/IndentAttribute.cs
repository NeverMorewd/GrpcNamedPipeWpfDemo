using MethodBoundaryAspect.Fody.Attributes;

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
