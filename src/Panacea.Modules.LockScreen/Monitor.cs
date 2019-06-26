using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Panacea.Modules.LockScreen
{
    public class Monitor
    {
        [DllImport("user32.dll")]
        static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);

        private const int MOUSEEVENTF_MOVE = 0x0001;
        /// <summary>
        ///     Puts the screen on.
        /// </summary>
        public static void on()
        {
            mouse_event(MOUSEEVENTF_MOVE, 0, 1, 0, UIntPtr.Zero);
            Thread.Sleep(40);
            mouse_event(MOUSEEVENTF_MOVE, 0, -1, 0, UIntPtr.Zero);
        }
        private static int WM_SYSCOMMAND = 0x0112;

        private static int SC_MONITORPOWER = 0xF170;
        //Using the system pre-defined MSDN constants that can be used by the SendMessage() function . 


        [DllImport("user32.dll")]
        private static extern int SendMessage(int hWnd, int hMsg, int wParam, IntPtr lParam);

        //To call a DLL function from C#, you must provide this declaration. 

        /// <summary>
        ///     Turns the screen Off
        /// </summary>
        public static String off(IntPtr handle)
        {
            try
            {
                //IntPtr handle = FindWindowByCaption(IntPtr.Zero, Console.Title.ToString());
                //Console.WriteLine("Monitor going off...");
                //System.Threading.Thread.Sleep(1000);
                SendMessage(handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, (IntPtr)2); //DLL function 
            }
            catch
            {
            }
            return "OK";
        }


        /// <summary>
        ///     Puts the screen in standby mode.
        /// </summary>
        public static String standby(IntPtr handle)
        {
            //IntPtr handle = FindWindowByCaption(IntPtr.Zero, Console.Title.ToString());
            //Console.WriteLine("Monitor going on Standby...");
            //System.Threading.Thread.Sleep(1000);
            SendMessage(handle.ToInt32(), WM_SYSCOMMAND, SC_MONITORPOWER, (IntPtr)1); //DLL function 
            return "OK";
        }

        /// <summary>
        /// Minimizes brightness
        /// </summary>
        public static bool MinimizeBrightness()
        {
            return ChangeBrightness(1);
        }

        /// <summary>
        /// Maximizes brightness
        /// </summary>
        public static bool MaximizeBrightness()
        {
            return ChangeBrightness(100);
        }


        internal static bool ChangeBrightness(int level)
        {
            try
            {
                var mclass = new ManagementClass("WmiMonitorBrightnessMethods");

                mclass.Scope = new ManagementScope(@"\\.\root\wmi");

                var instances = mclass.GetInstances();

                foreach (ManagementObject instance in instances)
                {
                    var brightness = level;
                    var timeout = level;
                    var args = new object[] { brightness, timeout };
                    instance.InvokeMethod("WmiSetBrightness", args);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
