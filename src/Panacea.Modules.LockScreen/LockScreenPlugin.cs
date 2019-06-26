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

namespace Panacea.Modules.LockScreen
{
    public class LockScreenPlugin : IPlugin
    {
        private PanaceaServices _core;

        public LockScreenPlugin(PanaceaServices core)
        {
            _core = core;
        }

        public Task BeginInit()
        {
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
            return Task.CompletedTask;
        }

        private void SetupBoundTerminalListeners()
        {
            if (_core.TryGetPairingPlugin(out IPairingPlugin pairing))
            {
                if (pairing.GetBoundTerminalManager().IsBound())
                {
                    pairing.GetBoundTerminalManager().GetBoundTerminal().On<dynamic>("lockscreen", async (obj) =>
                    {
                        switch ((string)obj.Action.ToString())
                        {
                            case "turnscreenoff":
                                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PANACEA_SCREEN_ACTIVE")))
                                {
                                    if(_core.TryGetUiManager(out IUiManager ui))
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
                                    if(_core.TryGetMediaPlayerContainer(out IMediaPlayerContainer container)){
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
                    });
                }
            }
        }
        public Task Shutdown()
        {
            return Task.CompletedTask;
        }
    }
}
