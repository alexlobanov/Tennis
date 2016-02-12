using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tennis_Betfair
{
    public static class SimulateMouseClick
    {
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        [DllImport("user32.dll")]
        private static extern void mouse_event(
            uint dwFlags, // motion and click options
            uint dx, // horizontal position or change
            uint dy, // vertical position or change
            uint dwData, // wheel movement
            IntPtr dwExtraInfo // application-defined information
            );

        public static void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            var X = uint.Parse(Cursor.Position.X.ToString());
            var Y = uint.Parse(Cursor.Position.Y.ToString());
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, (IntPtr.Zero));
        }
    }
}