using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;

namespace ElementPlayer.Core.ViewModels
{
    public class PlaylistViewModel : BaseViewModel
    {
        public PlaylistViewModel(ILoggerFactory logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
        }
    }
}
