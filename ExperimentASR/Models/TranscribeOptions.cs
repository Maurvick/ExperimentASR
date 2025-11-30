namespace ExperimentASR.Models
{
    public class TranscribeOptions
    {
        public enum WhisperModelSize
        {
            Tiny = 0,
            Base = 1,
            Small = 2,
            Medium = 3,
            Large = 4
        }
        public enum AudioLanguage
        {
            English = 0,
            Ukrainian = 1,
            Polish = 2,
            Czech = 3,
            German = 4,
            None = 5
        }
    }
}
