using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;

namespace ElementPlayer.Core.ViewModels
{
    public class QueueViewModel : BaseViewModel
    {
        public QueueViewModel(ILoggerFactory logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
        }

        public override string Title => "Queue";
    }
}
