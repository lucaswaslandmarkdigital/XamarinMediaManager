using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;

namespace ElementPlayer.Core.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel(ILoggerFactory logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
        }

        public override string Title => "Settings";
    }
}
