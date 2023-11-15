using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoClient.Common
{
    public class Global : ReactiveObject
    {
        private const string _DefaultChannelName = "53189515-EE63-427A-870D-F3D11BD96F36";
        private Global()
        {
            ChannelName = _DefaultChannelName;
        }

        public static readonly Global Singleton = new();

        [Reactive]
        public string ConsoleOutPut
        {
            get;
            set;
        } = string.Empty;

        public string ChannelName
        {
            get;
            set;
        }

    }
}
