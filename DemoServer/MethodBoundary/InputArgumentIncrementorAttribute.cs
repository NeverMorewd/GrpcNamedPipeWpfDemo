using MethodBoundaryAspect.Fody.Attributes;

namespace DemoServer.MethodBoundary
{
    [AllowChangingInputArguments]
    public sealed class InputArgumentIncrementorAttribute : OnMethodBoundaryAspect
    {
        public int Increment { get; set; }

        public override void OnEntry(MethodExecutionArgs args)
        {
            var inputArguments = args.Arguments;
            for (var i = 0; i < inputArguments.Length; i++)
            {
                var value = inputArguments[i];
                if (value is int v)
                    inputArguments[i] = v + Increment;
            }
        }
    }
}
