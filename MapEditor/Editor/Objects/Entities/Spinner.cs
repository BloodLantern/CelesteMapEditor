using Editor.Celeste;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Editor.Objects.Entities
{
    public class Spinner : Entity
    {
        public class BackgroundSpinner : Spinner
        {
            public override Vector2 Position => base.Position + Offset;
            public Vector2 Offset;
            public readonly Spinner Spinner;

            public BackgroundSpinner(Spinner spinner, Vector2 offset) : base(spinner.EntityData, spinner.Level)
            {
                Offset = offset - Size;
                Spinner = spinner;
                Layer = "bg";
            }
        }

        public const string BasePath = "danger/crystal/{layer}_{color}";

        public string Color;
        public readonly bool Dust = false;
        public readonly bool AttachToSolid;

        public readonly List<Spinner> Neighbors = [];
        public readonly List<BackgroundSpinner> BackgroundSpinners = [];

        protected string Layer;

        public Spinner(EntityData data, Level level) : base(data, level)
        {
            Color = data.Attr("color", "Blue");
            Dust = data.Bool("dust");
            AttachToSolid = data.Bool("attachToSolid");
            Layer = "fg";
        }

        public override void UpdateTexture()
        {
            if (Dust)
            {
                // Dustbunny
                //return;
                Color = "Red";
            }

            string path = BasePath.Replace("{layer}", Layer).Replace("{color}", Color.ToLower());
            Texture = Atlas.Gameplay[path + "00"];
        }

        public void CreateBackgroundSpinners(List<Spinner> spinners)
        {
            foreach (Spinner otherSpinner in spinners)
            {
                if (otherSpinner == this)
                    continue;

                Vector2 thisToOther = otherSpinner.Position - Position;

                if (!(thisToOther.Length() < 24f))
                    continue;
                
                Neighbors.Add(otherSpinner);

                if (AttachToSolid == otherSpinner.AttachToSolid && otherSpinner.Position.X >= Position.X)
                    BackgroundSpinners.Add(new(this, thisToOther * 0.5f));
            }
        }
    }
}
