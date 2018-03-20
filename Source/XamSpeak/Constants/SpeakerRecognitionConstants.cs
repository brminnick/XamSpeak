using System;
using Xamarin.Forms;

namespace XamSpeak
{
    public static class SpeakerRecognitionConstants
    {
        public const float SpeakerRecognitionSampleRate = 16000;
        public const int SpeakerRecognitionChannels = 1;
        public const int SpeakerRecognitionBitDepth = 16;
        public const string SpeakerRecognitionFormat = "wav";
        static readonly Lazy<int> _speakerRecognitionAudioFormatHolder = new Lazy<int>(DependencyService.Get<IAudioRecorder>().GetSpeakerRecognitionAudioFormat);

        public static int SpeakerRecognitionAudioFormat => _speakerRecognitionAudioFormatHolder.Value;
    }
}
