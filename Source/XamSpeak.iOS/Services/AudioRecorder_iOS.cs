using System;
using System.IO;

using AudioToolbox;
using AVFoundation;
using Foundation;

using Xamarin.Forms;

using XamSpeak.iOS;

[assembly: Dependency(typeof(AudioRecorder_iOS))]
namespace XamSpeak.iOS
{
    public class AudioRecorder_iOS : IAudioRecorder
    {
        #region Fields
        bool _isInitialized;
        string _audioFilePath;
        AVAudioRecorder _recorder;
        NSError _error;
        NSUrl _url;
        NSMutableDictionary _settings;
        #endregion

        #region Methods
        public int GetSpeakerRecognitionAudioFormat() => (int)AudioFormatType.LinearPCM;

        public void BeginRecording()
        {
            if (!_isInitialized)
                Init();

            var currentDateTimeAsString = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
            var fileName = string.Format($"AudioFile_{currentDateTimeAsString}.{SpeakerRecognitionConstants.SpeakerRecognitionFormat}");
            _audioFilePath = Path.Combine(Path.GetTempPath(), fileName);

            _url = NSUrl.FromFilename(_audioFilePath);

            _recorder = AVAudioRecorder.Create(_url, new AudioSettings(_settings), out _error);
            _recorder.PrepareToRecord();
            _recorder.Record();
        }

        public byte[] FinishRecording()
        {
            _recorder.Stop();

            byte[] audioFileAsByteArray;

            using (var streamReader = new StreamReader(_audioFilePath))
            using (var memstream = new MemoryStream())
            {
                streamReader.BaseStream.CopyTo(memstream);
                audioFileAsByteArray = memstream.ToArray();
            }

            File.Delete(_audioFilePath);

            return audioFileAsByteArray;
        }

        void Init()
        {
            _settings = new NSMutableDictionary
            {
                { AVAudioSettings.AVSampleRateKey, NSNumber.FromFloat(SpeakerRecognitionConstants.SpeakerRecognitionSampleRate) },
                { AVAudioSettings.AVFormatIDKey, NSNumber.FromInt32(SpeakerRecognitionConstants.SpeakerRecognitionAudioFormat) },
                { AVAudioSettings.AVNumberOfChannelsKey, NSNumber.FromInt32(SpeakerRecognitionConstants.SpeakerRecognitionChannels) },
                { AVAudioSettings.AVLinearPCMBitDepthKey, NSNumber.FromInt32(SpeakerRecognitionConstants.SpeakerRecognitionBitDepth) },
                { AVAudioSettings.AVLinearPCMIsBigEndianKey, NSNumber.FromBoolean(false) },
                { AVAudioSettings.AVLinearPCMIsFloatKey, NSNumber.FromBoolean(false) }
            };

            var audioSession = AVAudioSession.SharedInstance();
            var err = audioSession.SetCategory(AVAudioSessionCategory.PlayAndRecord);

            _isInitialized &= err == null;

            err = audioSession.SetActive(true);

            _isInitialized &= err == null;
        }
        #endregion
    }
}
