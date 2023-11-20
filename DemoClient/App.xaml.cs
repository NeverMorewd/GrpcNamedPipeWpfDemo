using DemoClient.Common;
using MaterialDesignThemes.Wpf;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows;

namespace DemoClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal string? StartupPage { get; set; }
        internal FlowDirection InitialFlowDirection { get; set; }
        internal BaseTheme InitialTheme { get; set; }

        public App()
        {
            var consoleWriter = new ConsoleOutputWriter();
            Console.SetOut(consoleWriter);
            Global.Singleton.SetConsoleObservable(consoleWriter.OutputObservable);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            (StartupPage, InitialFlowDirection, InitialTheme) = CommandLineOptions.ParseCommandLine(e.Args);

            //This is an alternate way to initialize MaterialDesignInXAML if you don't use the MaterialDesignResourceDictionary in App.xaml
            //Color primaryColor = SwatchHelper.Lookup[MaterialDesignColor.DeepPurple];
            //Color accentColor = SwatchHelper.Lookup[MaterialDesignColor.Lime];
            //ITheme theme = Theme.Create(new MaterialDesignLightTheme(), primaryColor, accentColor);
            //Resources.SetTheme(theme);


            //Illustration of setting culture info fully in WPF:
            /*             
            Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                        XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            */

            //XamlDisplay.Init();

            // test setup for Persian culture settings
            /*System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("fa-Ir");
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fa-Ir");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(
                        System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));*/

            base.OnStartup(e);
            Console.WriteLine("OnStartup");
        }
    }
}
