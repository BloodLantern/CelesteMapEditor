using Editor.Celeste;

namespace Editor.Entities
{
    public class Spikes : Entity
    {
        private static readonly string spikesTextureBaseName = "danger/spikes/{type}_{direction}00";

        public Spikes(EntityData data, Level level) : base(data, level)
        {
        }

        public override void UpdateTexture()
        {
            Texture = Atlas.Gameplay[spikesTextureBaseName.Replace("{type}", EntityData.Attr("type", "default")).Replace("{direction}", Name[6..].ToLower())];
        }
    }
}
