namespace Editor
{
    public class AudioState
    {
        public static string[] LayerParameters = new string[10]
        {
            "layer0",
            "layer1",
            "layer2",
            "layer3",
            "layer4",
            "layer5",
            "layer6",
            "layer7",
            "layer8",
            "layer9"
        };
        public AudioTrackState Music = new();
        public AudioTrackState Ambience = new();

        public AudioState()
        {
        }

        public AudioState(AudioTrackState music, AudioTrackState ambience)
        {
            if (music != null)
                Music = music.Clone();
            if (ambience == null)
                return;
            Ambience = ambience.Clone();
        }

        public AudioState(string music, string ambience)
        {
            Music.Event = music;
            Ambience.Event = ambience;
        }

        public AudioState Clone() => new()
        {
            Music = Music.Clone(),
            Ambience = Ambience.Clone()
        };
    }
}
