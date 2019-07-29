using Xamarin.Forms;

namespace XamSpeak
{
    class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel, new()
    {
        #region Constructors
        public BaseContentPage()
        {
            BindingContext = ViewModel;
            BackgroundColor = ColorConstants.ContentPageBackgroundColor;
            this.SetBinding(IsBusyProperty, nameof(BaseViewModel.IsInternetConnectionActive));
        }
        #endregion

        #region Properties
        protected TViewModel ViewModel { get; } = new TViewModel();
        #endregion
    }
}
