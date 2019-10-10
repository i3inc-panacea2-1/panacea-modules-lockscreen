using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.LockScreen.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Panacea.Modules.LockScreen.ViewModels
{
    [View(typeof(LockBlankPage))]
    class LockBlankPageViewModel : ViewModelBase
    {
        public event EventHandler CloseRequest;
        private readonly PanaceaServices _core;
        private readonly bool requirelogin;
        private IntPtr win;
        public string Password { get; set; }
        public Visibility BlackGridVisibility { get; set; }
        public LockBlankPageViewModel(PanaceaServices core, bool requirelogin)
        {
            _core = core;
            this.requirelogin = requirelogin;
            WrongPassBlockVisibility = Visibility.Hidden;
           
            TurnOffScreenCommand = new RelayCommand(args =>
            {
                TurnoffClick();
            });
            SignoutCommand = new RelayCommand(args =>
            {
                SignoutClick();
            });
            UnlockCommand = new RelayCommand(args =>
            {
                unlockClick((args as PasswordBox).Password);
            });
        }

        public RelayCommand TurnOffScreenCommand { get; set; }
        public RelayCommand SignoutCommand { get; set; }
        public RelayCommand UnlockCommand { get; set; }
        public Visibility WrongPassBlockVisibility { get; private set; }

        private void unlockClick(string Password)
        {
            WrongPassBlockVisibility = Visibility.Hidden;

            if (Password != _core.UserService.User.Password){
                WrongPassBlockVisibility = Visibility.Visible;
                OnPropertyChanged(nameof(WrongPassBlockVisibility));
                //if (Password.Length != 4)
                //{
                //}
                return;
            }
            OnPropertyChanged(nameof(WrongPassBlockVisibility));
            CloseRequest?.Invoke(this, null);
        }

        public override void Activate()
        {
            var w = Window.GetWindow(this.View);
            var h = new WindowInteropHelper(w);
            Monitor.off(h.Handle);
        }

        public override void Deactivate()
        {
            Monitor.on();
        }

        private async void SignoutClick()
        {
            await _core.UserService.LogoutAsync();
            CloseRequest?.Invoke(this, null);
        }

        private void TurnoffClick()
        {
            BlackGridVisibility = Visibility.Visible;
            OnPropertyChanged(nameof(BlackGridVisibility));
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PANACEA_SCREEN_ACTIVE")))
            {
                Monitor.off(win);
            }
            else
            {
                Monitor.MinimizeBrightness();
            }
        }
    }
}
