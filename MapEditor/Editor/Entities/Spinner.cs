using Editor.Celeste;
using System.Collections.Generic;

namespace Editor.Entities
{
    public class Spinner : Entity
    {
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

        public Spinner(EntityData data, Level level) : base(data, level)
        {
        }

        public override void UpdateTexture()
        {
            if (EntityData.Bool("dust"))
            {
                // Dustbunny
                return;
            }

            Texture = spinnerTextureLookupTable[EntityData.Attr("color", "Blue")];
        }
    }
}
