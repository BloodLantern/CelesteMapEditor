using Editor.Celeste;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Editor
{
    public class Decal
    {
        public Texture Texture;
        public Vector2 Position;
        public Vector2 Scale;

        public Decal(DecalData data)
        {
            Texture = Atlas.Gameplay["decals/" + data.Texture.Replace('\\', '/').Replace(Path.GetExtension(data.Texture), "")];
            Position = data.Position;
            Scale = data.Scale;
        }

        public void Render(SpriteBatch spriteBatch, Camera camera, Vector2 levelPosition)
            => Texture.Render(spriteBatch, camera, Position + levelPosition - Texture.Size.ToVector2() * Scale / 2, Color.White, Scale);
    }
}
