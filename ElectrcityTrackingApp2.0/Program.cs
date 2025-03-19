using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ElectrcityTrackingApp2._0
{
    internal static class Program
    {
        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int awareness);
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SetProcessDpiAwareness(2);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new log_in());
        }
    }
}
