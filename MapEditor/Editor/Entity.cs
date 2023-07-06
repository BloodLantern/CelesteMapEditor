using Editor.Celeste;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Numerics;

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

        public EntityData EntityData { get; private set; }
        public Texture Texture;

        public string Name => EntityData.Name;
        public Vector2 Position => EntityData.Position;
        public Vector2 AbsolutePosition => Level.Position + EntityData.Position;
        public Size Size => Texture != null && EntityData.Size == Size.Empty ? Texture.Size : EntityData.Size;

        public Level Level;

        public Entity(EntityData data, Level level)
        {
            EntityData = data;
            Level = level;

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

        public void Render(RectangleF cameraBounds, Image<Rgba32> image)
        {
            PointF relativePosition = new(AbsolutePosition.X - cameraBounds.X, AbsolutePosition.Y - cameraBounds.Y);

            if (Session.CurrentSession.Config.ShowDebugInfo)
                image.Mutate(o => o.DrawPolygon(Color.Red, 2,
                    new PointF(relativePosition.X, relativePosition.Y),
                    new PointF(relativePosition.X + (Texture != null ? Texture.DrawOffset.X : 0) + Size.Width, relativePosition.Y),
                    new PointF(relativePosition.X + (Texture != null ? Texture.DrawOffset.X : 0) + Size.Width, relativePosition.Y + (Texture != null ? Texture.DrawOffset.Y : 0) + Size.Height),
                    new PointF(relativePosition.X, relativePosition.Y + (Texture != null ? Texture.DrawOffset.Y : 0) + Size.Height)));

            if (Texture == null
                || relativePosition.X + Texture.DrawOffset.X + Texture.ClipRect.Width <= 0
                || relativePosition.Y + Texture.DrawOffset.Y + Texture.ClipRect.Height <= 0
                || relativePosition.X + Texture.DrawOffset.X >= cameraBounds.Width
                || relativePosition.Y + Texture.DrawOffset.Y >= cameraBounds.Height)
                return;

            image.Mutate(o => o.DrawImage(Texture.Image, Texture.DrawOffset + (Size) (Point) relativePosition, 1f));
        }

        private Texture GetSpinnerTexture()
        {
            if (EntityData.Bool("dust"))
            {
                // Dustbunny
                return null;
            }

            return spinnerTextureLookupTable[EntityData.Attr("color", "Blue")];
        }

        private Texture GetSpikesTexture()
        {
            string direction = Name.Substring(6);

            return Atlas.Gameplay[spikesTextureBaseName.Replace("{type}", EntityData.Attr("type", "default")).Replace("{direction}", direction.ToLower())];
        }
    }
}
