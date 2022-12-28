using Microsoft.Extensions.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;

namespace ElementPlayer.Core.ViewModels
{
    public class BaseViewModel : MvxNavigationViewModel
    {
        public BaseViewModel(ILoggerFactory logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
        }

        public virtual string Title => "ElementPlayer";
    }

    public abstract class BaseViewModel<TParameter> : BaseViewModel, IMvxViewModel<TParameter>
    {
        public BaseViewModel(ILoggerFactory logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
        }

        public abstract void Prepare(TParameter parameter);
    }
}
