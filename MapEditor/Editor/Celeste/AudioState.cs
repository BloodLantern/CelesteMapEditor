﻿namespace Editor.Celeste
{
    public class AudioState
    {
        public static string[] LayerParameters =
        [
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
        ];
        public string Music;
        public string Ambience;

        public AudioState(string music, string ambience)
        {
            Music = music;
            Ambience = ambience;
        }
    }
}
