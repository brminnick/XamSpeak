using Xamarin.Forms;

namespace XamSpeak
{
	public class BaseContentPage<TViewModel> : ContentPage where TViewModel : BaseViewModel, new()
	{
		TViewModel _viewModel;

		public BaseContentPage()
		{
			BindingContext = ViewModel;
			BackgroundColor = Color.FromHex("A4EBE2");
		}

		protected TViewModel ViewModel => _viewModel ?? (_viewModel = new TViewModel());
	}
}
