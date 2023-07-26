using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Editor.Celeste
{
    /// <summary>
    /// The metadata of a chapter side.
    /// </summary>
    public class AreaData
    {
        public string Name;
        public string Icon;
        public bool Interlude;
        public string CompleteScreenName;
        public ModeProperties Mode;
        public Color TitleBaseColor = Color.White;
        public Color TitleAccentColor = Color.Gray;
        public Color TitleTextColor = Color.White;
        public IntroType IntroType;
        public bool Dreaming;
        public bool OverrideASideMeta = false;
        public string ColorGrade;
        public string Wipe;
        public float DarknessAlpha = 0.05f;
        public float BloomBase;
        public float BloomStrength = 1f;
        public string CassetteSong = "event:/music/cassette/01_forsaken_city";
        public CoreMode CoreMode;
        public string SpritesXmlPath;
        /// <summary>
        /// Whether the player is allowed to hold something while in a bubble (or feather).
        /// </summary>
        public bool HoldableInBubble;

        public AreaData(BinaryPacker.Element data)
        {
            foreach (KeyValuePair<string, object> pair in data.Attributes)
            {
                switch (pair.Key)
                {
                    case "BloomBase":
                        BloomBase = (float) pair.Value;
                        break;

                    case "BloomStrength":
                        BloomStrength = (float) pair.Value;
                        break;

                    case "CassetteSong":
                        CassetteSong = (string) pair.Value;
                        break;

                    case "CompleteScreenName":
                        CompleteScreenName = pair.Value as string;
                        break;

                    case "CoreMode":
                        CoreMode = (CoreMode) Enum.Parse(typeof(CoreMode), pair.Value as string);
                        break;

                    case "DarknessAlpha":
                        DarknessAlpha = (float) pair.Value;
                        break;

                    case "Dreaming":
                        Dreaming = (bool) pair.Value;
                        break;

                    case "Icon":
                        Icon = pair.Value as string;
                        break;

                    case "Interlude":
                        Interlude = (bool) pair.Value;
                        break;

                    case "IntroType":
                        IntroType = (IntroType) Enum.Parse(typeof(IntroType), pair.Value as string);
                        break;

                    case "Name":
                        Name = pair.Value as string;
                        continue;

                    case "OverrideASideMeta":
                        OverrideASideMeta = (bool) pair.Value;
                        break;

                    case "Sprites":
                        SpritesXmlPath = pair.Value as string;
                        break;

                    case "TitleAccentColor":
                        TitleAccentColor = Calc.HexToColor(pair.Value as string);
                        break;

                    case "TitleBaseColor":
                        TitleBaseColor = Calc.HexToColor(pair.Value as string);
                        break;

                    case "TitleTextColor":
                        TitleTextColor = Calc.HexToColor(pair.Value as string);
                        break;

                    case "Wipe":
                        Wipe = pair.Value as string;
                        break;
                }
            }

            Mode = new(data.Children[0]);
        }
    }
}
