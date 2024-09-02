using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Editor.Celeste;
using System.Collections.Generic;
using MonoGame.Extended;
using ImGuiNET;

namespace Editor.Objects
{
    public class Entity : MapObject
    {
        private static readonly Dictionary<string, Texture> TextureLookupTable = new()
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

        public Texture Texture;

        public string Name
        {
            get => EntityData.Name;
            set => EntityData.Name = value;
        }
        public override Vector2 Position
        {
            get => EntityData.Position - Size / 2f;
            set => EntityData.Position = value + Size / 2f;
        }
        public override Vector2 Size => Texture != null && EntityData.Size == Vector2.Zero ? Texture.Size.ToVector2() : EntityData.Size;

        public Entity(EntityData data, Level level)
            : base(level, MapObjectType.Entity)
        {
            EntityData = data;
        }

        public virtual void UpdateTexture()
        {
            if (Atlas.Gameplay.Textures.ContainsKey(Name))
            {
                Texture = Atlas.Gameplay[Name];
            }
            else if (TextureLookupTable.TryGetValue(Name, out Texture value))
            {
                Texture = value;
            }
            else
            {
                // TODO don't hardcode this
                if (Name.StartsWith("summitgem"))
                    Texture = TextureLookupTable[Name + EntityData.Int("gem")];
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
            => spriteBatch.DrawRectangle(camera.MapAreaToWindow(AbsoluteBounds), Color.Red, camera.GetLineThickness());

        public override void DebugInfo()
        {
            base.DebugInfo();

            ImGui.Text($"Name: '{Name}'");
            int attributeCount = EntityData.Attributes.Count;
            ImGui.Text($"Attribute count: {attributeCount}");
            
            if (attributeCount <= 0 || !ImGui.TreeNode("Attributes"))
                return;
            
            foreach (KeyValuePair<string, object> attribute in EntityData.Attributes)
                ImGui.Text($"{attribute.Key}: {attribute.Value}");
            ImGui.TreePop();
        }
    }
}
