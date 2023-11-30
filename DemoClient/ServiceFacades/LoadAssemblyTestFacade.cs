                       using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DemoClient.ServiceFacades
{
    internal class TestServiceFacade
    {
        public TestServiceFacade()
        {
            Memory<int> memory = new Memory<int>();
            //fixed
            Marshal.SizeOf(memory);
            //Marshall
            Span<byte> buffer = new Span<byte>();
        }
    }
}
