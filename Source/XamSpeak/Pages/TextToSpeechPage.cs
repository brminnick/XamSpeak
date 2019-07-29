using System;
using System.Threading.Tasks;
using Plugin.Permissions;

using Xamarin.Forms;

namespace XamSpeak
{
    class TextToSpeechPage : BaseContentPage<TextToSpeechViewModel>
    {
        public TextToSpeechPage()
        {
            var takePictureButton = new TakePhotoButton { Text = "Take A Picture Of Text" };
            takePictureButton.SetBinding(IsVisibleProperty, nameof(TextToSpeechViewModel.IsTakePictureButtonVisible));
            takePictureButton.SetBinding(Button.CommandProperty, nameof(TextToSpeechViewModel.TakePictureButtonCommand));

            var spokenTextLabel = new Label { HorizontalTextAlignment = TextAlignment.Center };
            spokenTextLabel.SetBinding(Label.TextProperty, nameof(TextToSpeechViewModel.SpokenTextLabelText));

            var activityIndicatorLabel = new Label { FontAttributes = FontAttributes.Italic };
            activityIndicatorLabel.SetBinding(Label.TextProperty, nameof(TextToSpeechViewModel.ActivityIndicatorLabelText));

            var activityIndicator = new ActivityIndicator { Color = ColorConstants.NavigationBarBackgroundColor };
            activityIndicator.SetBinding(IsVisibleProperty, nameof(TextToSpeechViewModel.IsActivityIndicatorDisplayed));
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(TextToSpeechViewModel.IsActivityIndicatorDisplayed));

            var stackLayout = new StackLayout
            {
                Margin = new Thickness(20, 0),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Children = {
                    spokenTextLabel,
                    activityIndicatorLabel,
                    activityIndicator,
                    takePictureButton,
                }
            };

            ViewModel.OCRFailed += HandleOCRFailed;
            ViewModel.SpellCheckFailed += HandleSpellCheckFailed;
            MediaServices.NoCameraDetected += HandleNoCameraDetected;
            MediaServices.PermissionsDenied += HandlePermissionsDenied;
            OCRServices.InvalidComputerVisionAPIKey += HandleInvalidComputerVisionAPIKey;
            SpellCheckServices.InvalidBingSpellCheckAPIKey += HandleInvalidBingSpellCheckAPIKey;
            ViewModel.InternetConnectionUnavailable += HandleInternetConnectionUnavailable;
            SpellCheckServices.Error429_TooManySpellCheckAPIRequests += HandleError429_TooManySpellCheckAPIRequests;

            Content = new ScrollView { Content = stackLayout };

            Title = "XamSpeak";
        }

        #region Methods
        async void HandlePermissionsDenied(object sender, EventArgs e)
        {
            var isAlertAccepted = await DisplayAlertOnMainThread("Open Settings?", "Storage and Camera Permission Need To Be Enabled", "Ok", "Cancel");
            if (isAlertAccepted)
                Device.BeginInvokeOnMainThread(() => CrossPermissions.Current.OpenAppSettings());
        }

        async void HandleInternetConnectionUnavailable(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Internet Connection Unavailable");
        async void HandleInvalidComputerVisionAPIKey(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Invalid Computer Vision API Key");
        async void HandleInvalidBingSpellCheckAPIKey(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Invalid Bing Spell Check API Key");
        async void HandleSpellCheckFailed(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Spell Check Failed");
        async void HandleNoCameraDetected(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "No Camera Available");
        async void HandleError429_TooManySpellCheckAPIRequests(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Bing Spell Check API Limit Reached");
        async void HandleOCRFailed(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Optical Character Recognition Failed");

        Task DisplayAlertOnMainThread(string title, string message) => Device.InvokeOnMainThreadAsync(() => DisplayAlert(title, message, "Ok"));
        Task<bool> DisplayAlertOnMainThread(string title, string message, string accept, string cancel) => Device.InvokeOnMainThreadAsync(() => DisplayAlert(title, message, accept, cancel));
        #endregion
    }
}
