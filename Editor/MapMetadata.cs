using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Editor
{
    /// <summary>
    /// The metadata of a chapter side.
    /// </summary>
    public class MapMetadata
    {
        public string Name;
        public string Icon;
        public int ID;
        public bool Interlude;
        public string CompleteScreenName;
        public ModeProperties[] Mode;
        public Color TitleBaseColor = Color.White;
        public Color TitleAccentColor = Color.Gray;
        public Color TitleTextColor = Color.White;
        public IntroType IntroType;
        public bool Dreaming;
        public string ColorGrade;
        //public Action<Scene, bool, Action> Wipe;
        public float DarknessAlpha = 0.05f;
        public float BloomBase;
        public float BloomStrength = 1f;
        public string CassetteSong = "event:/music/cassette/01_forsaken_city";
        public CoreMode CoreMode;
    }
}
