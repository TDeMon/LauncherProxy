using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LauncherProxy
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(params string[] args)
        {
            Application.ThreadException += Application_ThreadException;

            // Add handler to handle the exception raised by additional threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PreloaderApp(args));
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                MessageBox.Show((e.ExceptionObject as Exception).Message);
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                MessageBox.Show(e.Exception.Message);
            }
        }
    }
}
