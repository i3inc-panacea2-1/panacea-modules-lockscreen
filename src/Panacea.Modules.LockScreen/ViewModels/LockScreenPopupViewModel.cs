using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.TerminalPairing;
using Panacea.Modularity.UiManager;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Panacea.Modules.LockScreen.Views;

namespace Panacea.Modules.LockScreen.ViewModels
{
    [View(typeof(LockScreenPopup))]
    public class LockScreenPopupViewModel : PopupViewModelBase<object>
    {
        private PanaceaServices _core;
        private bool _requireLogin;

        public LockScreenPopupViewModel(PanaceaServices core, bool requireLogin)
        {
            _core = core;
            _requireLogin = requireLogin;
            LockScreenCommandVisibility = _requireLogin ? Visibility.Visible : Visibility.Collapsed;
            RemoteButtonsVisibility = Visibility.Collapsed;
            if(core.TryGetPairingPlugin(out IPairingPlugin pairing))
            {
                if (pairing.GetBoundTerminalManager().IsBound())
                {
                    RemoteButtonsVisibility = Visibility.Visible;
                };
            }
            LockScreenCommand = new RelayCommand(args =>
            {
                lockScreen();
                SetResult(null);
            });
            TurnOffScreenCommand = new RelayCommand(args =>
            {
                turnOffScreen();
                SetResult(null);
            });
            LockRemoteScreenCommand = new RelayCommand(args =>
            {
                lockRemoteScreen();
                SetResult(null);
            });
            UnlockRemoteScreenCommand = new RelayCommand(args =>
            {
                unlockRemoteScreen();
                SetResult(null);
            });
        }
        public RelayCommand LockScreenCommand { get; }
        public RelayCommand TurnOffScreenCommand { get; }
        public RelayCommand LockRemoteScreenCommand { get; }
        public RelayCommand UnlockRemoteScreenCommand { get; }

        public Visibility LockScreenCommandVisibility { get; }
        public Visibility RemoteButtonsVisibility { get; private set; }

        private void lockScreen()
        {
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                LockBlankPageViewModel viewModel = new LockBlankPageViewModel(_core, true);
                Window newWindow = GetFullScreenWindow(viewModel.View);
                viewModel.CloseRequest += (object sender, EventArgs e) =>
                {
                    newWindow.Close();
                };
            }
        }

        private void turnOffScreen()
        {
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                LockBlankPageViewModel viewModel = new LockBlankPageViewModel(_core, false);
                Window newWindow = GetFullScreenWindow(viewModel.View);
                viewModel.CloseRequest += (object sender, EventArgs e) =>
                {
                    newWindow.Close();
                };
            }
        }
        private void lockRemoteScreen()
        {
            if (_core.TryGetPairingPlugin(out IPairingPlugin pairing))
            {
                pairing.GetBoundTerminalManager()?.GetBoundTerminal()?.Send("lockscreen", new { Action = "turnscreenoff" });
            }
        }
        private void unlockRemoteScreen()
        {
            if (_core.TryGetPairingPlugin(out IPairingPlugin pairing))
            {
                pairing.GetBoundTerminalManager()?.GetBoundTerminal()?.Send("lockscreen", new { Action = "turnscreenoff" });
            }
        }
        private Window GetFullScreenWindow(FrameworkElement view)
        {
            Window w = new Window();
            w.Topmost = true;
            w.Content = view;
            w.WindowState = WindowState.Maximized;
            w.WindowStyle = WindowStyle.None;
            w.ResizeMode = ResizeMode.NoResize;
            w.Show();
            return w;
        }
    }
}
