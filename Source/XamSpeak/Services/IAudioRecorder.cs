namespace XamSpeak
{
    public interface IAudioRecorder
    {
        void BeginRecording();
        byte[] FinishRecording();
        int GetSpeakerRecognitionAudioFormat();
    }
}
