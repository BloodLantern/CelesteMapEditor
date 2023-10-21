using Editor.Objects;
using System.Collections.Generic;

namespace Editor.Edits
{
    public abstract class Edit
    {
        public readonly List<MapObject> AffectedObjects = new();

        public bool Done { get; private set; } = false;

        public abstract void Do();

        public abstract void Undo();

        public void Redo() => Do();
    }
}
