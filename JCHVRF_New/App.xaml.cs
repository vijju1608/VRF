using System;
using System.Globalization;
using System.Windows;

namespace JCHVRF_New
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            JCBase.Utility.MyConfig.AppPath = AppDomain.CurrentDomain.BaseDirectory;
            JCBase.Util.CultureHelper.SetCurrentCultrue(new CultureInfo("en-US"));

            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
               
        private static void Dispatcher_UnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                Logger.LogProjectError(null, e.Exception);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogProjectError(null, e.ExceptionObject as Exception, !e.IsTerminating);
        }
    }
}
