using System;
using Xamarin.Forms;

namespace XamSpeak
{
	public class TextToSpeechPage : BaseContentPage<TextToSpeechViewModel>
	{
		public TextToSpeechPage()
		{
			var takePictureButton = new TakePhotoButton { Text = "   Take A Picture Of Text   " };
			var inverseBoolConverter = new InverseBoolConverter();
			var inverseBoolBinding = new Binding(nameof(ViewModel.IsActivityIndicatorDisplayed), BindingMode.Default, inverseBoolConverter, ViewModel.IsActivityIndicatorDisplayed);
			takePictureButton.SetBinding(IsVisibleProperty, inverseBoolBinding);
			takePictureButton.SetBinding(Button.CommandProperty, nameof(ViewModel.TakePictureButtonCommand));


			var spokenTextLabel = new Label { HorizontalTextAlignment = TextAlignment.Center };
			spokenTextLabel.SetBinding(Label.TextProperty, nameof(ViewModel.SpokenTextLabelText));

			var activityIndicatorLabel = new Label { FontAttributes = FontAttributes.Italic };
			activityIndicatorLabel.SetBinding(Label.TextProperty, nameof(ViewModel.ActivityIndicatorLabelText));

			var activityIndicator = new ActivityIndicator();
			activityIndicator.SetBinding(IsVisibleProperty, nameof(ViewModel.IsActivityIndicatorDisplayed));
			activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ViewModel.IsActivityIndicatorDisplayed));

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children = {
					spokenTextLabel,
					activityIndicatorLabel,
					activityIndicator,
					takePictureButton,
				}
			};

			Title = "XamSpeak";
		}

		#region Methods
		protected override void OnAppearing()
		{
			base.OnAppearing();

			ViewModel.NoCameraDetected += HandleNoCameraDetected;
			HttpHelpers.Error429_TooManySpellCheckAPIRequests += HandleError429_TooManySpellCheckAPIRequests;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			ViewModel.NoCameraDetected -= HandleNoCameraDetected;
			HttpHelpers.Error429_TooManySpellCheckAPIRequests -= HandleError429_TooManySpellCheckAPIRequests;

		}

		void HandleNoCameraDetected(object sender, EventArgs e)
		{
			Device.BeginInvokeOnMainThread(async () =>
										   await DisplayAlert("Error", "No Camera Available", "Ok"));
		}

		void HandleError429_TooManySpellCheckAPIRequests(object sender, EventArgs e)
		{
			Device.BeginInvokeOnMainThread(async () =>
											await DisplayAlert("Error", "Bing Spell Check API Limit Reached", "Ok"));
		}
		#endregion
	}
}
