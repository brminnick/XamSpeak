using System;
using Xamarin.Forms;
namespace XamSpeak
{
    public class SelectionPage : BaseContentPage<BaseViewModel>
    {
        #region Constant Fields
        readonly XamSpeakButton _textToSpeechButton, _speakerIdentificationButton;
        #endregion

        public SelectionPage()
        {
            _textToSpeechButton = new XamSpeakButton { Text = "   Text To Speech   " };
            _speakerIdentificationButton = new XamSpeakButton { Text = "   Speaker Identification   " };

            Title = "XamSpeak";

            Content = new StackLayout
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children ={
                    _textToSpeechButton,
                    _speakerIdentificationButton
                }
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _textToSpeechButton.Clicked += HandleTextToSpeechButtonClicked;
            _speakerIdentificationButton.Clicked += HandleSpeakerIdentificationButtonClicked;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _textToSpeechButton.Clicked -= HandleTextToSpeechButtonClicked;
            _speakerIdentificationButton.Clicked -= HandleSpeakerIdentificationButtonClicked;
        }

        void HandleSpeakerIdentificationButtonClicked(object sender, EventArgs e) =>
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushAsync(new SpeakerIdentificationPage()));

        void HandleTextToSpeechButtonClicked(object sender, EventArgs e) =>
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushAsync(new TextToSpeechPage()));
    }
}
