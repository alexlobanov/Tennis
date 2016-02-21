using System;
using System.Reflection.Emit;
using System.Threading;
using System.Windows.Forms;

namespace Tennis_Betfair
{
    internal static class Program
    {
        /// <summary>
        ///     Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

         /*   Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //ловим все исключения
            Application.ThreadException += new ThreadExceptionEventHandler(
                CheckUnhandledEx.ApplicationOnThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                CheckUnhandledEx.CurrentDomain_UnhandledException);*/

            Application.Run(new MainForm());
        }
    }
}