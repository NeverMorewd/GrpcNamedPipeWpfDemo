using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace DemoClient
{
    public class ConsoleOutputWriter : TextWriter
    {
        private readonly TextWriter consoleOutput;
        private readonly Subject<string?> _outputSubject;

        public ConsoleOutputWriter()
        {
            consoleOutput = Console.Out;
            _outputSubject = new Subject<string?>();
        }

        public override void WriteLine(string? value)
        {
            consoleOutput.WriteLine(value);
            _outputSubject.OnNext(value);
        }

        public override Encoding Encoding => consoleOutput.Encoding;

        public IObservable<string?> OutputObservable => _outputSubject.AsObservable();
    }
}
