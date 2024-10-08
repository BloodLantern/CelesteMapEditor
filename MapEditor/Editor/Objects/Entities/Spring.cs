﻿using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Editor.Objects.Entities
{
    public class Spring : Entity
    {
        public Spring(EntityData data, Level level)
            : base(data, level)
        {
        }

        public override void UpdateTexture()
        {
            Texture = new(Atlas.Gameplay["objects/spring/00"]);
            Texture.Origin = new(Texture.Width * 0.5f, Texture.Height);

            string nameLower = Name.ToLower();
            if (!nameLower.Contains("wall"))
                return;
            
            if (nameLower.Contains("left"))
                Texture.Rotation = MathHelper.PiOver2;
            else if (nameLower.Contains("right"))
                Texture.Rotation = 3f * MathHelper.PiOver2;
        }

        public override void Render(SpriteBatch spriteBatch, Camera camera)
            => Texture.Render(spriteBatch, camera, AbsolutePosition + Size * 0.5f, Color.White);
    }
}
