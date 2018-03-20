using System;
namespace XamSpeak
{
    public interface IAudioRecorder
    {
        void BeginRecording(Guid audioFileGuid);
        byte[] FinishRecording(Guid audioFileGuid);
        int GetSpeakerRecognitionAudioFormat();
        bool IsRecording(Guid audioFileGuid);
    }
}
