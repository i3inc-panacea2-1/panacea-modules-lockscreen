using Panacea.Core;
using Panacea.Modularity;
using Panacea.Modularity.TerminalPairing;
using Panacea.Modularity.UiManager;
using Panacea.Modules.LockScreen.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using Panacea.Modularity.MediaPlayerContainer;
using System.Windows.Threading;
using System.Windows;
using Panacea.Modules.LockScreen.Views;
using System.Diagnostics;

namespace Panacea.Modules.LockScreen
{
    public class LockScreenPlugin : IPlugin
    {
        private PanaceaServices _core;
        private readonly MonitorHook _hook;
        public static TapToUnlockWindow _unlockWindow;

        public LockScreenPlugin(PanaceaServices core)
        {
            _core = core;
            _hook = new MonitorHook();
            _hook.ScreenOn += _hook_ScreenOn;
        }

        private void _hook_ScreenOn(object sender, EventArgs e)
        {
            Debug.WriteLine(DateTime.Now.ToLongTimeString());
            Application.Current.Dispatcher.Invoke(async() =>
            {
                await Task.Delay(2000);
                _unlockWindow?.Hide();
            });
        }

        public Task BeginInit()
        {
            _hook.Start();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            return;
        }

        public Task EndInit()
        {
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.AddNavigationBarControl(new LockScreenButtonViewModel(_core));
            }
            else
            {
                _core.Logger.Error(this, "ui manager not loaded");
            }
            SetupBoundTerminalListeners();
            return Task.CompletedTask;
        }

        private async void OnAction(dynamic obj)
        {
            switch ((string)obj["Action"].ToString())
            {
                case "turnscreenoff":
                    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PANACEA_SCREEN_ACTIVE")))
                    {
                        if (_core.TryGetUiManager(out IUiManager ui))
                        {
                            Monitor.off(new WindowInteropHelper(Window.GetWindow(ui as FrameworkElement)).Handle);
                        }
                    }
                    else
                    {
                        Monitor.MinimizeBrightness();
                    }
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_core.TryGetMediaPlayerContainer(out IMediaPlayerContainer container))
                        {
                            container.CurrentMediaPlayer?.Stop();
                        }
                    }));
                    break;

                case "turnscreenon":
                    Monitor.on();
                    Monitor.MaximizeBrightness();
                    break;
                case "signout":
                    if (_core.UserService.User.Id != null)
                    {
                        await _core.UserService.SetUser(null);
                    }
                    break;
            }
        }

        private void SetupBoundTerminalListeners()
        {
            if (_core.TryGetPairing(out IBoundTerminalManager pairing))
            {
                if (pairing.IsBound())
                {
                    pairing.GetBoundTerminal().On<dynamic>("lockscreen", OnAction);
                }
            }
        }
        public Task Shutdown()
        {
            _hook.Stop();
            if (_core.TryGetPairing(out IBoundTerminalManager pairing))
            {
                if (pairing.IsBound())
                {
                    pairing.GetBoundTerminal().Off<dynamic>("lockscreen", OnAction);
                }
            }
            return Task.CompletedTask;
        }
    }
}
