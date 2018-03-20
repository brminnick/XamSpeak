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

            _textToSpeechButton.Clicked += HandleSpeakTextFromAPhotoButtonClicked;
            _speakerIdentificationButton.Clicked += HandleIdentifySpeakerFromAudioButtonClicked;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _textToSpeechButton.Clicked -= HandleSpeakTextFromAPhotoButtonClicked;
            _speakerIdentificationButton.Clicked -= HandleIdentifySpeakerFromAudioButtonClicked;
        }

        void HandleIdentifySpeakerFromAudioButtonClicked(object sender, EventArgs e) =>
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushAsync(new SpeakerIdentificationPage()));

        void HandleSpeakTextFromAPhotoButtonClicked(object sender, EventArgs e) =>
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushAsync(new TextToSpeechPage()));
    }
}
