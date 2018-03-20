using System;
using System.Collections.Generic;
using System.IO;

using AudioToolbox;
using AVFoundation;
using Foundation;

using Xamarin.Forms;

using XamSpeak.iOS;
using System.Linq;

[assembly: Dependency(typeof(AudioRecorder_iOS))]
namespace XamSpeak.iOS
{
    public class AudioRecorder_iOS : IAudioRecorder
    {
        #region Constant Fields
        readonly Lazy<Dictionary<Guid, AVAudioRecorder>> _recorderDictionaryHolder = new Lazy<Dictionary<Guid, AVAudioRecorder>>();
        readonly Lazy<Dictionary<Guid, string>> _audioFilePathDictionaryHolder = new Lazy<Dictionary<Guid, string>>();
        readonly Lazy<Dictionary<Guid, bool>> _isRecordingDictionaryHolder = new Lazy<Dictionary<Guid, bool>>();
        #endregion

        #region Fields
        bool _isInitialized;
        NSError _error;
        NSUrl _url;
        NSMutableDictionary _speakerRecognitionSettings;
        #endregion

        #region Properties
        Dictionary<Guid, AVAudioRecorder> RecorderDictionary => _recorderDictionaryHolder.Value;
        Dictionary<Guid, string> AudioFilePathDictionary => _audioFilePathDictionaryHolder.Value;
        Dictionary<Guid, bool> IsRecordingDictionary => _isRecordingDictionaryHolder.Value;
        #endregion

        #region Methods
        public int GetSpeakerRecognitionAudioFormat() => (int)AudioFormatType.LinearPCM;

        public bool IsRecording(Guid audioFileGuid)
        {
            var isRecordingKeyValuePair = IsRecordingDictionary.Where(x => x.Key.Equals(audioFileGuid))?.FirstOrDefault();
            return isRecordingKeyValuePair?.Value ?? false;
        }

        public void BeginRecording(Guid audioFileGuid)
        {
            if (!_isInitialized)
                Init();

            var currentDateTimeAsString = DateTimeOffset.Now.ToString("yyyyMMddHHmmss");
            var fileName = string.Format($"{currentDateTimeAsString}.{SpeakerRecognitionConstants.SpeakerRecognitionFormat}");
            var audioFilePath = Path.Combine(Path.GetTempPath(), fileName);

            _url = NSUrl.FromFilename(audioFilePath);

            var recorder = AVAudioRecorder.Create(_url, new AudioSettings(_speakerRecognitionSettings), out _error);

            RecorderDictionary.Add(audioFileGuid, recorder);
            AudioFilePathDictionary.Add(audioFileGuid, audioFilePath);

            recorder.PrepareToRecord();
            recorder.Record();

            IsRecordingDictionary.Add(audioFileGuid, true);
        }

        public byte[] FinishRecording(Guid audioFileGuid)
        {
            var recorder = RecorderDictionary[audioFileGuid];
            var audioFilePath = AudioFilePathDictionary[audioFileGuid];

            recorder.Stop();
            IsRecordingDictionary.Add(audioFileGuid, false);

            byte[] audioFileAsByteArray;

            using (var streamReader = new StreamReader(audioFilePath))
            using (var memstream = new MemoryStream())
            {
                streamReader.BaseStream.CopyTo(memstream);
                audioFileAsByteArray = memstream.ToArray();
            }

            File.Delete(audioFilePath);

            return audioFileAsByteArray;
        }

        void Init()
        {
            _speakerRecognitionSettings = new NSMutableDictionary
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
