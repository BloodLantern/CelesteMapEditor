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
            {
                "summitgem0",
                Atlas.Gameplay["collectables/summitgems/0/gem00"]
            },
            {
                "summitgem1",
                Atlas.Gameplay["collectables/summitgems/1/gem00"]
            },
            {
                "summitgem2",
                Atlas.Gameplay["collectables/summitgems/2/gem00"]
            },
            {
                "summitgem3",
                Atlas.Gameplay["collectables/summitgems/3/gem00"]
            },
            {
                "summitgem4",
                Atlas.Gameplay["collectables/summitgems/4/gem00"]
            },
            {
                "summitgem5",
                Atlas.Gameplay["collectables/summitgems/5/gem00"]
            },
        };

        public readonly EntityData EntityData;
        public readonly Level Level;

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
            else
            {
                // TODO don't hardcode this
                if (Name.StartsWith("summitgem"))
                    Texture = textureLookupTable[Name + EntityData.Int("gem")];
            }
        }

        public virtual void Render(SpriteBatch spriteBatch, Camera camera)
        {
            if (Texture != null)
                Texture.Render(spriteBatch, camera, AbsolutePosition);
            else
                RenderDebug(spriteBatch, camera);
        }

        public void RenderDebug(SpriteBatch spriteBatch, Camera camera)
            => spriteBatch.DrawRectangle(camera.MapToWindow(Bounds), Color.Red, camera.GetLineThickness());

        public virtual void DebugInfo()
        {
            ImGui.Text($"Name: '{Name}'");
            ImGui.Text($"Bounds: {Bounds}");
            int attributeCount = EntityData.Attributes.Count;
            ImGui.Text($"Attribute count: {attributeCount}");
            if (attributeCount > 0 && ImGui.TreeNode("Attributes"))
            {
                foreach (KeyValuePair<string, object> attribute in EntityData.Attributes)
                    ImGui.Text($"{attribute.Key}: {attribute.Value}");
                ImGui.TreePop();
            }
        }
    }
}
