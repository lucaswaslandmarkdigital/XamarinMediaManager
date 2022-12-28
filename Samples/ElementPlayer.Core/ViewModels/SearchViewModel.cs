using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;

namespace ElementPlayer.Core.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        public SearchViewModel(ILoggerFactory logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
        }

        public override string Title => "Search";
    }
}
