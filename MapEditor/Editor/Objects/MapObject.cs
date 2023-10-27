using Editor.Celeste;
using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Editor.Objects
{
    public class MapObject
    {
        /// <summary>
        /// The position relative to the level this <see cref="MapObject"/> is in.
        /// This needs to be virtual as some objects or entities like <see cref="Entities.JumpThru"/>s
        /// need to override this so that it is only <see cref="EntityData.Position"/>.
        /// </summary>
        public virtual Vector2 Position { get; set; }
        public Vector2 AbsolutePosition => Level.Position.ToVector2() + Position;
        public virtual Point Size { get; set; }
        public Vector2 Center => AbsolutePosition + Size.ToVector2() / 2;

        public RectangleF Bounds => new(Position, Size);
        public RectangleF AbsoluteBounds => new(AbsolutePosition, Size);

        public readonly MapObjectType Type;

        public readonly Level Level;

        public MapObject(Level level, MapObjectType type = MapObjectType.Object)
        {
            Level = level;
            Type = type;
        }

        public MapObject(Level level, Vector2 offset, MapObjectType type = MapObjectType.Object) : this(level, type) => Position = offset;

        public virtual void DebugInfo() => ImGui.Text($"Absolute bounds: {AbsoluteBounds}");

        public void RemoveFromMap() => Level.Remove(this);
    }
}
