using System;
using System.Reflection;

namespace DemoClient
{
    public static class MethodTimeLogger
    {
        public static void Log(MethodBase methodBase, TimeSpan elapsed, string message)
        {
            Console.WriteLine($"{methodBase.Name}:{elapsed.TotalMilliseconds}ms.");
        }
    }
}
