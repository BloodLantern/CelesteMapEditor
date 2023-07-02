using System.Collections.Generic;

namespace Editor.Celeste
{
    public class ModeProperties
    {
        public string PoemID;
        public string StartLevel;
        public PlayerInventory Inventory;
        public AudioState AudioState;
        public bool IgnoreLevelAudioLayerData;
        public bool HeartIsEnd;
        public bool SeekerSlowdown;
        public bool TheoInBubble;

        public ModeProperties(BinaryPacker.Element data)
        {
            foreach (KeyValuePair<string, object> pair in data.Attributes)
            {
                switch (pair.Key)
                {
                    case "HeartIsEnd":
                        HeartIsEnd = (bool) pair.Value;
                        break;

                    case "Inventory":
                        Inventory = PlayerInventory.Parse(pair.Value as string);
                        break;
                    case "IgnoreLevelAudioLayerData":
                        IgnoreLevelAudioLayerData = (bool) pair.Value;
                        break;

                    case "SeekerSlowdown":
                        SeekerSlowdown = (bool) pair.Value;
                        break;

                    case "StartLevel":
                        StartLevel = pair.Value as string;
                        break;

                    case "TheoInBubble":
                        TheoInBubble = (bool) pair.Value;
                        break;
                }
            }

            string audioMusic = null, audioAmbience = null;
            foreach (KeyValuePair<string, object> pair in data.Children[0].Attributes)
            {
                switch (pair.Key)
                {
                    case "Ambience":
                        audioAmbience = pair.Value as string;
                        break;

                    case "Music":
                        audioMusic = pair.Value as string;
                        break;
                }
            }

            AudioState = new(audioMusic, audioAmbience);
        }
    }
}
