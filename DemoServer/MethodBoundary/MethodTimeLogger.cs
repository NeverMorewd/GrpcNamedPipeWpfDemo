using System.Reflection;

namespace DemoServer.MethodBoundary
{
    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            Console.WriteLine();
            Console.WriteLine($"{methodBase.Name}:{elapsed.TotalMilliseconds}ms.");
        }
    }
}
