using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.UiManager;
using Panacea.Modules.LockScreen.Views;
using Panacea.Mvvm;

namespace Panacea.Modules.LockScreen.ViewModels
{
    [View(typeof(LockScreenButton))]
    public class LockScreenButtonViewModel : ViewModelBase
    {
        private PanaceaServices core;

        public LockScreenButtonViewModel(PanaceaServices core)
        {
            this.core = core;
            LockCommand = new RelayCommand(args =>
            {
                LockScreenPopupViewModel lockScreen;
                if (core.UserService.User.Id == null)
                {
                    lockScreen = new LockScreenPopupViewModel(core, false);
                }
                else
                {
                    lockScreen = new LockScreenPopupViewModel(core, true);
                }
                if (core.TryGetUiManager(out IUiManager ui))
                {
                    ui.ShowPopup<object>(lockScreen);
                }
            });
        }

        public RelayCommand LockCommand { get; set; }
    }
}
