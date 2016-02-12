using System;
using System.Diagnostics;
using System.Threading;

namespace Tennis_Betfair
{
    public static class CheckUnhandledEx
    {
        public static void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Debug.WriteLine("EXCEPTION!! Msg: " + e.Exception.Message + "\n Stack trace: \n" + e.Exception.StackTrace +
                "\n sourse: \n" + e.Exception.Source);
        }
        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                string errorMsg = "An application error occurred. Please contact the adminstrator " +
                              "with the following information:\n\n";
                Exception ex = (Exception)e.ExceptionObject;
                Debug.WriteLine(errorMsg + ex.Message + "\n\nStack Trace:\n" + ex.StackTrace
                    + "\n sourse: \n" + ex.Source);
            }
            catch (Exception exc)
            {
                var errorMsg = "Fatal Non-UI Error. Could not write the error to the event log. Reason: "
                 + exc.Message;
                Debug.WriteLine(errorMsg);
            }
        }
    }
}