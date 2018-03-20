using System;
using Xamarin.Forms;

namespace XamSpeak
{
    public class SpeakerIdentificationPage : BaseContentPage<SpeakerIdentificationViewModel>
    {
        readonly XamSpeakButton _recordButton;

        public SpeakerIdentificationPage()
        {
            _recordButton = new XamSpeakButton
            {
                Text = "   Record   ",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            Title = "Speaker Identification";

            Content = _recordButton;
        }
    }
}
