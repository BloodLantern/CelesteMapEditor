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
            public Vector2 Offset = Vector2.Zero;
            public readonly Spinner Spinner;

            public BackgroundSpinner(Spinner spinner, Vector2 offset) : base(spinner.EntityData, spinner.Level)
            {
                Offset = offset - Size.ToVector2();
                Spinner = spinner;
                Layer = "bg";
            }
        }

        public const string BasePath = "danger/crystal/{layer}_{color}";

        public string Color = "Blue";
        public bool Dust = false;
        public bool AttachToSolid;

        public readonly List<Spinner> Neighbors = new();
        public readonly List<BackgroundSpinner> BackgroundSpinners = new();

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

                if (thisToOther.Length() < 24f)
                {
                    Neighbors.Add(otherSpinner);

                    if (AttachToSolid == otherSpinner.AttachToSolid && otherSpinner.Position.X >= Position.X)
                        BackgroundSpinners.Add(new BackgroundSpinner(this, thisToOther / 2f));
                }

            }
        }
    }
}
