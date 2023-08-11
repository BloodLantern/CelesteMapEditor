using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Editor.Extensions
{
    public static class SpriteBatchExt
    {
        public static void DrawString(this SpriteBatch self, Dictionary<char, Texture> font, string text, Camera camera, Vector2 position, Color color, float scale)
        {
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (!font.ContainsKey(c))
                    continue;
                Texture texture = font[char.ToUpper(c)];
                texture.Render(self, camera, position + new Vector2(texture.Size.X + 1, 0) * i, color, scale);
            }
        }
    }
}
