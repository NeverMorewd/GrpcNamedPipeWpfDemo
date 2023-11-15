using System;
using System.IO;
using System.Text;

namespace DemoClient
{
    public class ConsoleOutputWriter : TextWriter
    {
        private readonly TextWriter consoleOutput;
        private readonly Action<string> outputTextAction;

        public ConsoleOutputWriter(Action<string> anOutputTextAction)
        {
            consoleOutput = Console.Out;
            outputTextAction = anOutputTextAction;
        }

        public override void WriteLine(string? value)
        {
            consoleOutput.WriteLine(value);
            outputTextAction?.Invoke($"{value}{Environment.NewLine}");
        }

        public override Encoding Encoding => consoleOutput.Encoding;
    }
}
