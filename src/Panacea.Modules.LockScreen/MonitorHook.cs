using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Panacea.Modules.LockScreen
{
    class MonitorHook
    {
        EventNativeWindow _window;
        IntPtr _ScreenStateNotify;
        public event EventHandler ScreenOn;
        public event EventHandler ScreenOff;

        public void Start()
        {
            if (_window != null) return;
            _window = new EventNativeWindow();
            _window.Message += _window_Message;
            _window.CreateHandle(new CreateParams());
            _ScreenStateNotify = NativeMethods.RegisterPowerSettingNotification(
                _window.Handle,
                ref NativeMethods.GUID_CONSOLE_DISPLAY_STATE,
                NativeMethods.DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        public void Stop()
        {
            if (_window == null) return;
            NativeMethods.UnregisterPowerSettingNotification(_ScreenStateNotify);
            _window.DestroyHandle();
            _window.ReleaseHandle();
            _window = null;
        }

        private void _window_Message(object sender, Message e)
        {
            // handler of console display state system event 
            if (e.Msg == NativeMethods.WM_POWERBROADCAST)
            {
                if (e.WParam.ToInt32() == NativeMethods.PBT_POWERSETTINGCHANGE)
                {
                    var s = (NativeMethods.POWERBROADCAST_SETTING)Marshal.PtrToStructure(e.LParam, typeof(NativeMethods.POWERBROADCAST_SETTING));
                    if (s.PowerSetting == NativeMethods.GUID_CONSOLE_DISPLAY_STATE)
                    {
                        switch (s.Data)
                        {
                            case 0x0:
                                ScreenOff?.Invoke(this, null);
                                break;
                            case 0x1:
                                ScreenOn?.Invoke(this, null);
                                break;
                        }
                    }
                }
            }


        }
    }

    internal class EventNativeWindow : NativeWindow
    {
        public event EventHandler<Message> Message;
        protected override void WndProc(ref Message m)
        {
            Message?.Invoke(this, m);
            base.WndProc(ref m);
        }
    }

    internal static class NativeMethods
    {
        public static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid(0x6fe69556, 0x704a, 0x47a0, 0x8f, 0x24, 0xc2, 0x8d, 0x93, 0x6f, 0xda, 0x47);
        public const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        public const int WM_POWERBROADCAST = 0x0218;
        public const int PBT_POWERSETTINGCHANGE = 0x8013;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);



        [DllImport(@"User32", SetLastError = true, EntryPoint = "UnregisterPowerSettingNotification", CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnregisterPowerSettingNotification(IntPtr handle);
    }
}
