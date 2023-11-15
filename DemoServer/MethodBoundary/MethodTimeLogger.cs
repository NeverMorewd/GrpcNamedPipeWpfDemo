using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
