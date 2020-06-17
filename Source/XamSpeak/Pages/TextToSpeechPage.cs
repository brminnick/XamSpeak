using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Markup;

namespace XamSpeak
{
    class TextToSpeechPage : BaseContentPage<TextToSpeechViewModel>
    {
        public TextToSpeechPage()
        {
            ViewModel.OCRFailed += HandleOCRFailed;
            ViewModel.SpellCheckFailed += HandleSpellCheckFailed;
            MediaService.NoCameraDetected += HandleNoCameraDetected;
            MediaService.PermissionsDenied += HandlePermissionsDenied;
            OCRServices.InvalidComputerVisionAPIKey += HandleInvalidComputerVisionAPIKey;
            SpellCheckServices.InvalidBingSpellCheckAPIKey += HandleInvalidBingSpellCheckAPIKey;
            ViewModel.InternetConnectionUnavailable += HandleInternetConnectionUnavailable;
            SpellCheckServices.Error429_TooManySpellCheckAPIRequests += HandleError429_TooManySpellCheckAPIRequests;

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Margin = new Thickness(20, 0),
                    Children =
                    {
                        new Label().TextCenterHorizontal()
                            .Bind(Label.TextProperty, nameof(TextToSpeechViewModel.SpokenTextLabelText)),
                        new Label { FontAttributes = FontAttributes.Italic }
                            .Bind(Label.TextProperty, nameof(TextToSpeechViewModel.ActivityIndicatorLabelText)),
                        new ActivityIndicator { Color = ColorConstants.NavigationBarBackgroundColor }
                            .Bind(IsVisibleProperty, nameof(TextToSpeechViewModel.IsActivityIndicatorDisplayed))
                            .Bind(ActivityIndicator.IsRunningProperty, nameof(TextToSpeechViewModel.IsActivityIndicatorDisplayed)),
                        new TakePhotoButton("Take A Picture Of Text")
                            .Bind(IsVisibleProperty, nameof(TextToSpeechViewModel.IsTakePictureButtonVisible))
                            .Bind(Button.CommandProperty, nameof(TextToSpeechViewModel.TakePictureButtonCommand))
                    }
                }.CenterExpand()
            };

            Title = "XamSpeak";
        }

        async void HandlePermissionsDenied(object sender, EventArgs e)
        {
            var isAlertAccepted = await DisplayAlertOnMainThread("Open Settings?", "Storage and Camera Permission Need To Be Enabled", "Ok", "Cancel");
            if (isAlertAccepted)
                await MainThread.InvokeOnMainThreadAsync(AppInfo.ShowSettingsUI);
        }

        async void HandleInternetConnectionUnavailable(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Internet Connection Unavailable");
        async void HandleInvalidComputerVisionAPIKey(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Invalid Computer Vision API Key");
        async void HandleInvalidBingSpellCheckAPIKey(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Invalid Bing Spell Check API Key");
        async void HandleSpellCheckFailed(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Spell Check Failed");
        async void HandleNoCameraDetected(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "No Camera Available");
        async void HandleError429_TooManySpellCheckAPIRequests(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Bing Spell Check API Limit Reached");
        async void HandleOCRFailed(object sender, EventArgs e) => await DisplayAlertOnMainThread("Error", "Optical Character Recognition Failed");

        Task DisplayAlertOnMainThread(string title, string message) => MainThread.InvokeOnMainThreadAsync(() => DisplayAlert(title, message, "Ok"));
        Task<bool> DisplayAlertOnMainThread(string title, string message, string accept, string cancel) => MainThread.InvokeOnMainThreadAsync(() => DisplayAlert(title, message, accept, cancel));

        class TakePhotoButton : Button
        {
            public TakePhotoButton(in string text)
            {
                Text = text;
                TextColor = Color.White;
                Padding = new Thickness(10, 0);
                FontAttributes = FontAttributes.Bold;
                BackgroundColor = ColorConstants.NavigationBarBackgroundColor;
            }
        }
    }
}
