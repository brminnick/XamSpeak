using System;
using System.Windows.Input;
using Xamarin.Forms;
namespace XamSpeak
{
    public class SpeakerIdentificationViewModel : BaseViewModel
    {
        #region Constant Fields
        const string _isRecordingText = "      Recording...       ";
        const string _defaultAudioButtonText = "         Record          ";
        const string _identifyingSpeakerText = "   Identifying Speaker   ";
        readonly Guid _recordButtonGuid = Guid.NewGuid();
        #endregion

        #region Fields
        string _recordButtonText;
        bool _isRecordButtonEnabled;
        ICommand _recordButtonCommand;
        #endregion

        #region Properties
        ICommand RecordButtonCommand => _recordButtonCommand ??
            (_recordButtonCommand = new Command(ExecuteRecordButtonCommand));

        public string RecordButtonText
        {
            get => _recordButtonText;
            set => SetProperty(ref _recordButtonText, value);
        }

        public bool IsRecordButtonEnabled
        {
            get => _isRecordButtonEnabled;
            set => SetProperty(ref _isRecordButtonEnabled, value);
        }
        #endregion

        #region Mehthods
        void ExecuteRecordButtonCommand()
        {
            byte[] audioFile;

            var isRecording = DependencyService.Get<IAudioRecorder>().IsRecording(_recordButtonGuid);

            if (isRecording)
            {
                IsRecordButtonEnabled = false;
                DependencyService.Get<IAudioRecorder>().BeginRecording(_recordButtonGuid);
                RecordButtonText = _isRecordingText;
            }
            else
            {
                RecordButtonText = _identifyingSpeakerText;
                audioFile = DependencyService.Get<IAudioRecorder>().FinishRecording(_recordButtonGuid);

                var speaker = 
            }
        }
        #endregion
    }
}
