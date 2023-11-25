using Editor.Celeste;
using Editor.Objects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Editor
{
    public class Decal : MapObject
    {
        public readonly DecalData DecalData;
        public Texture Texture;
        public Vector2 Scale;

        public override Point Size => (Texture.Size.ToVector2() * Scale).ToPoint();

        public Decal(Level level, DecalData data)
            : base(level, MapObjectType.Decal)
        {
            DecalData = data;
            Texture = Atlas.Gameplay["decals/" + data.Texture.Replace('\\', '/').Replace(Path.GetExtension(data.Texture), "")];
            Scale = data.Scale;
            Position = data.Position;
        }

        public void Render(SpriteBatch spriteBatch, Camera camera)
            => Texture.Render(spriteBatch, camera, Position + Level.Position.ToVector2() - Texture.Size.ToVector2() * Scale / 2, Color.White, Scale);
    }
}
