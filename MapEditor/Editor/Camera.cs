using Editor.Utils;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Editor
{
    public class Camera
    {
        public const float DefaultZoom = 1f;

        public RectangleF Bounds;
        public Vector2 Position { get => Bounds.Position; set => Bounds.Position = value; }
        public Vector2 Size { get => Bounds.Size; set => Bounds.Size = value; }

        private float zoom = DefaultZoom;
        public float Zoom
        {
            get => zoom;
            set
            {
                if (value <= 0)
                    return;

                Size *= zoom / value;
                zoom = value;
            }
        }

        public Camera(RectangleF bounds) => Bounds = bounds;

        public Camera(Vector2 position, Vector2 size)
            : this(new RectangleF(position, size))
        {
        }

        public Vector2 MapToWindow(Vector2 position) => (position - Position) * Zoom;

        public Point MapToWindow(Point position) => (position - Position.ToPoint()).Mul(Zoom);

        public Vector2 WindowToMap(Vector2 position) => position / Zoom + Position;

        public Point WindowToMap(Point position) => position.Div(Zoom) + Position.ToPoint();
    }
}
