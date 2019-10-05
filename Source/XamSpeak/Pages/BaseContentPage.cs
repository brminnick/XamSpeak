using Xamarin.Forms;

namespace XamSpeak
{
    class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel, new()
    {
        public BaseContentPage()
        {
            BindingContext = ViewModel;
            BackgroundColor = ColorConstants.ContentPageBackgroundColor;
            this.SetBinding(IsBusyProperty, nameof(BaseViewModel.IsInternetConnectionActive));
        }

        protected TViewModel ViewModel { get; } = new TViewModel();
    }
}
