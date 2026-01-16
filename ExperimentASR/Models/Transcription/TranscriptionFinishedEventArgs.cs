namespace SpeechMaster.Models.Transcription
{
    // Event args for finished event
    public class TranscriptionFinishedEventArgs : EventArgs
    {
        public TranscriptionResult Result { get; }
        public TranscriptionFinishedEventArgs(TranscriptionResult result) => Result = result;
    }
}
