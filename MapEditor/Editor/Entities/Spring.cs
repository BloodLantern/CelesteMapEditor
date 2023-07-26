using Editor.Celeste;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Entities
{
    public class Spring : Entity
    {
        public float Rotation = 0f;

        public Vector2 Offset;
        public override Vector2 TextureAbsolutePosition => AbsolutePosition + Offset;

        public Spring(EntityData data, Level level)
            : base(data, level)
        {
            string nameLower = Name.ToLower();
            if (nameLower.Contains("wall"))
            {
                if (nameLower.Contains("left"))
                {
                    Rotation = MathHelper.PiOver2;
                    Offset = new(9.5f, 8f);
                }
                else if (nameLower.Contains("right"))
                {
                    Rotation = 3 * MathHelper.PiOver2;
                    Offset = new(6.5f, 8f);
                }
            }
            else
                Offset = new(2f, 5f);
        }

        public override void UpdateTexture()
        {
            Texture = Atlas.Gameplay["objects/spring/00"];
        }

        public override void Render(SpriteBatch spriteBatch, Camera camera)
        {
            Texture.Render(spriteBatch, camera, TextureAbsolutePosition, Color.White, rotation:Rotation);
        }

        public override void DebugInfo()
        {
            base.DebugInfo();
            ImGui.SliderAngle($"Rotation", ref Rotation);
        }
    }
}
