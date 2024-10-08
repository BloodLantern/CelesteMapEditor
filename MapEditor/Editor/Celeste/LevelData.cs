﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Editor.Celeste
{
    public class LevelData
    {
        public readonly string Name;
        public readonly bool Dummy;
        public int Strawberries;
        public bool HasGem;
        public bool HasHeartGem;
        public bool HasCheckpoint;
        public bool DisableDownTransition;
        public Rectangle Bounds;
        public readonly List<EntityData> Entities;
        public readonly List<EntityData> Triggers;
        public readonly List<Vector2> PlayerSpawns;
        public readonly List<DecalData> FgDecals;
        public readonly List<DecalData> BgDecals;
        public readonly string ForegroundTiles = "";
        public readonly string BackgroundTiles = "";
        public string FgTiles = "";
        public string BgTiles = "";
        public string ObjTiles = "";
        public WindPattern WindPattern;
        public Vector2 CameraOffset;
        public bool Dark;
        public bool Underwater;
        public bool Space;
        public string Music = "";
        public string AltMusic = "";
        public string Ambience = "";
        public readonly float[] MusicLayers = new float[4];
        public int MusicProgress = -1;
        public int AmbienceProgress = -1;
        public bool MusicWhispers;
        public bool DelayAltMusic;
        public int EnforceDashNumber;
        public readonly int EditorColorIndex;

        public LevelData(BinaryPacker.Element data)
        {
            Bounds = new();
            foreach (KeyValuePair<string, object> attribute in data.Attributes)
            {
                switch (attribute.Key)
                {
                    case "alt_music":
                        AltMusic = attribute.Value as string;
                        break;
                    case "ambience":
                        Ambience = attribute.Value as string;
                        break;
                    case "ambienceProgress":
                        string s1 = attribute.Value.ToString();
                        if (string.IsNullOrEmpty(s1) || !int.TryParse(s1, out AmbienceProgress))
                        {
                            AmbienceProgress = -1;
                            break;
                        }
                        break;
                    case "c":
                        EditorColorIndex = (int) attribute.Value;
                        break;
                    case "cameraOffsetX":
                        CameraOffset.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                        break;
                    case "cameraOffsetY":
                        CameraOffset.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                        break;
                    case "dark":
                        Dark = (bool) attribute.Value;
                        break;
                    case "delayAltMusicFade":
                        DelayAltMusic = (bool) attribute.Value;
                        break;
                    case "disableDownTransition":
                        DisableDownTransition = (bool) attribute.Value;
                        break;
                    case "enforceDashNumber":
                        EnforceDashNumber = (int) attribute.Value;
                        break;
                    case "height":
                        Bounds.Height = (int) attribute.Value;
                        /*if (Bounds.Height == 184)
                        {
                            Bounds.Height = 180;
                            break;
                        }*/ // ???
                        break;
                    case "music":
                        Music = (string) attribute.Value;
                        break;
                    case "musicLayer1":
                        MusicLayers[0] = (bool) attribute.Value ? 1f : 0.0f;
                        break;
                    case "musicLayer2":
                        MusicLayers[1] = (bool) attribute.Value ? 1f : 0.0f;
                        break;
                    case "musicLayer3":
                        MusicLayers[2] = (bool) attribute.Value ? 1f : 0.0f;
                        break;
                    case "musicLayer4":
                        MusicLayers[3] = (bool) attribute.Value ? 1f : 0.0f;
                        break;
                    case "musicProgress":
                        string s2 = attribute.Value.ToString();
                        if (string.IsNullOrEmpty(s2) || !int.TryParse(s2, out MusicProgress))
                        {
                            MusicProgress = -1;
                            break;
                        }
                        break;
                    case "name":
                        Name = attribute.Value as string;
                        break;
                    case "space":
                        Space = (bool) attribute.Value;
                        break;
                    case "underwater":
                        Underwater = (bool) attribute.Value;
                        break;
                    case "whisper":
                        MusicWhispers = (bool) attribute.Value;
                        break;
                    case "width":
                        Bounds.Width = (int) attribute.Value;
                        break;
                    case "windPattern":
                        WindPattern = (WindPattern) Enum.Parse(typeof(WindPattern), attribute.Value as string);
                        break;
                    case "x":
                        Bounds.X = (int) attribute.Value;
                        break;
                    case "y":
                        Bounds.Y = (int) attribute.Value;
                        break;
                    default:
                        break;
                }
            }

            PlayerSpawns = [];
            Entities = [];
            Triggers = [];
            BgDecals = [];
            FgDecals = [];

            foreach (BinaryPacker.Element child in data.Children)
            {
                if (child.Name == "entities")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element entity in child.Children)
                        {
                            if (entity.Name == "player")
                                PlayerSpawns.Add(new(Convert.ToSingle(entity.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(entity.Attributes["y"], CultureInfo.InvariantCulture)));
                            else if (entity.Name is "strawberry" or "snowberry")
                                Strawberries++;
                            else if (entity.Name == "shard")
                                HasGem = true;
                            else if (entity.Name == "blackGem")
                                HasHeartGem = true;
                            else if (entity.Name == "checkpoint")
                                HasCheckpoint = true;

                            if (entity.Name != "player")
                                Entities.Add(CreateEntityData(entity));
                        }
                }
                else if (child.Name == "triggers")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element trigger in child.Children)
                            Triggers.Add(CreateEntityData(trigger));
                }
                else if (child.Name == "bgdecals")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element decal in child.Children)
                            BgDecals.Add(
                                new()
                                {
                                    Position = new(Convert.ToSingle(decal.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["y"], CultureInfo.InvariantCulture)),
                                    Scale = new(Convert.ToSingle(decal.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["scaleY"], CultureInfo.InvariantCulture)),
                                    Texture = (string) decal.Attributes["texture"]
                                }
                            );
                }
                else if (child.Name == "fgdecals")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element decal in child.Children)
                            FgDecals.Add(
                                new()
                                {
                                    Position = new(Convert.ToSingle(decal.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["y"], CultureInfo.InvariantCulture)),
                                    Scale = new(Convert.ToSingle(decal.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["scaleY"], CultureInfo.InvariantCulture)),
                                    Texture = (string) decal.Attributes["texture"]
                                }
                            );
                }
                else if (child.Name == "solids")
                    ForegroundTiles = child.Attr("innerText");
                else if (child.Name == "bg")
                    BackgroundTiles = child.Attr("innerText");
                else if (child.Name == "fgtiles")
                    FgTiles = child.Attr("innerText");
                else if (child.Name == "bgtiles")
                    BgTiles = child.Attr("innerText");
                else if (child.Name == "objtiles")
                    ObjTiles = child.Attr("innerText");
            }

            Dummy = PlayerSpawns.Count == 0;
        }

        private EntityData CreateEntityData(BinaryPacker.Element entity)
        {
            EntityData result = new(entity.Name, this, new Vector2[entity.Children == null ? 0 : entity.Children.Count]);

            if (entity.Attributes != null)
                foreach (KeyValuePair<string, object> attribute in entity.Attributes)
                {
                    if (attribute.Key == "id")
                        result.Id = (int) attribute.Value;
                    else if (attribute.Key == "x")
                        result.Position.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "y")
                        result.Position.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "width")
                        result.Size.X = (int) attribute.Value;
                    else if (attribute.Key == "height")
                        result.Size.Y = (int) attribute.Value;
                    else if (attribute.Key == "originX")
                        result.Origin.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "originY")
                        result.Origin.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else
                    {
                        result.Attributes ??= new();
                        result.Attributes.Add(attribute.Key, attribute.Value);
                    }
                }

            for (int i = 0; i < result.Nodes.Length; i++)
                foreach (KeyValuePair<string, object> attribute in entity.Children[i].Attributes)
                {
                    if (attribute.Key == "x")
                        result.Nodes[i].X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "y")
                        result.Nodes[i].Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                }

            return result;
        }

        public bool Check(Vector2 at) => at.X >= Bounds.Left && at.Y >= Bounds.Top && at.X < Bounds.Right && at.Y < Bounds.Bottom;

        public Rectangle TileBounds => new(Bounds.X / 8, Bounds.Y / 8, (int) Math.Ceiling(Bounds.Width / 8.0), (int) Math.Ceiling(Bounds.Height / 8.0));

        public Point Position
        {
            get => new(Bounds.X, Bounds.Y);
            set
            {
                for (int index = 0; index < PlayerSpawns.Count; ++index)
                    PlayerSpawns[index] -= Position.ToVector2();
                Bounds.X = value.X;
                Bounds.Y = value.Y;
                for (int index = 0; index < PlayerSpawns.Count; ++index)
                    PlayerSpawns[index] += Position.ToVector2();
            }
        }
    }
}
