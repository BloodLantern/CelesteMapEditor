using Editor.Celeste;
using Editor.Graphics;
using System.Collections.Generic;

namespace Editor
{
    public class Entity
    {
        private static readonly Dictionary<string, Texture> textureLookupTable = new()
        {
            /*{
                "",
                Atlas.Gameplay[""]
            },*/
        };
        private static readonly Dictionary<string, Texture> spinnerTextureLookupTable = new()
        {
            {
                "Blue",
                Atlas.Gameplay["danger/crystal/fg_blue00"]
            },
            {
                "Red",
                Atlas.Gameplay["danger/crystal/fg_red00"]
            },
            {
                "Purple",
                Atlas.Gameplay["danger/crystal/fg_purple00"]
            },
            {
                "Rainbow",
                Atlas.Gameplay["danger/crystal/fg_white00"]
            },
        };
        private static readonly string spikesTextureBaseName = "danger/spikes/{type}_{direction}00";

        public EntityData EntityData;
        public Texture Texture;

        public string Name => EntityData.Name;

        public Entity(EntityData entityData)
        {
            EntityData = entityData;

            string shortName = Name;

            if (shortName.StartsWith("spikes"))
                shortName = "spikes";

            Texture = shortName switch
            {
                "spinner" => GetSpinnerTexture(),
                "spikes" => GetSpikesTexture(),
                _ => /*textureLookupTable[Name]*/null,
            };
        }

        private Texture GetSpinnerTexture()
        {
            if (EntityData.Bool("dust"))
            {
                // Dustbunny
                return null;
            }

            return spinnerTextureLookupTable[EntityData.Attr("color", "blue")];
        }

        private Texture GetSpikesTexture()
        {
            string direction = Name.Substring(6);

            return Atlas.Gameplay[spikesTextureBaseName.Replace("{type}", EntityData.Attr("type", "default")).Replace("{direction}", direction.ToLower())];
        }
    }
}
