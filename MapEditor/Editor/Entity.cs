using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;
using Editor.Celeste;
using System.Collections.Generic;
using MonoGame.Extended;
using ImGuiNET;

namespace Editor
{
    public class Entity
    {
        private static readonly Dictionary<string, Texture> textureLookupTable = new()
        {
            {
                "goldenBerry",
                Atlas.Gameplay["collectables/goldberry/idle00"]
            },
            {
                "strawberry",
                Atlas.Gameplay["collectables/strawberry/normal00"]
            },
            {
                "refill",
                Atlas.Gameplay["objects/refill/idle00"]
            },
        };

        public EntityData EntityData { get; private set; }
        public Texture Texture;

        public string Name => EntityData.Name;
        /// <summary>
        /// The position relative to the level this Entity is in.
        /// This needs to be virtual as some entities like <see cref="Entities.JumpThru"/>s
        /// need to override this so that it is only <see cref="EntityData.Position"/>.
        /// </summary>
        public virtual Vector2 Position => EntityData.Position - Size.ToVector2() / 2;
        public Vector2 AbsolutePosition => Level.Position.ToVector2() + Position;
        public virtual Point Size => Texture != null && EntityData.Size == Point.Zero ? Texture.Size : EntityData.Size;

        public RectangleF Bounds => new(AbsolutePosition, Size);

        public Level Level;

        public Entity(EntityData data, Level level)
        {
            EntityData = data;
            Level = level;
        }

        public virtual void UpdateTexture()
        {
            if (Atlas.Gameplay.Textures.ContainsKey(Name))
                Texture = Atlas.Gameplay[Name];
            else if (textureLookupTable.ContainsKey(Name))
                Texture = textureLookupTable[Name];
        }

        public virtual void Render(SpriteBatch spriteBatch, Camera camera)
        {
            if (Texture != null)
                Texture.Render(spriteBatch, camera, AbsolutePosition);
            else
                RenderDebug(spriteBatch, camera);
        }

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(
                new RectangleF(
                    camera.MapToWindow(AbsolutePosition),
                    Size.ToVector2() * camera.Zoom
                ),
                Color.Red
            );

        public virtual void DebugInfo()
        {
            ImGui.Text($"Name: '{Name}'");
            ImGui.Text($"Bounds: {Bounds}");
        }
    }
}
