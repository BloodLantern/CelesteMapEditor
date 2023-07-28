using Editor.Celeste;
using Editor.Extensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Editor.Entities
{
    public class Spinner : Entity
    {
        public class BackgroundSpinner : Spinner
        {
            public override Vector2 Position => EntityData.Position + Offset;
            public Vector2 Offset = Vector2.Zero;
            public readonly Spinner Spinner;

            public BackgroundSpinner(Spinner spinner, Vector2 offset) : base(spinner.EntityData, spinner.Level)
            {
                Offset = offset;
                Spinner = spinner;
            }

            public override void UpdateTexture()
            {
                if (Dust)
                {
                    // Dustbunny
                    //return;
                    Color = "Red";
                }

                string path = BasePath.Replace("{layer}", "bg").Replace("{color}", Color.ToLower());
                Texture = Atlas.Gameplay[path + "00"];
                Texture.Origin = Texture.Size.ToVector2() / 2;
            }
        }

        public const string BasePath = "danger/crystal/{layer}_{color}";

        public string Color = "Blue";
        public bool Dust = false;
        public bool AttachToSolid;

        public Spinner(EntityData data, Level level) : base(data, level)
        {
            Color = data.Attr("color", "Blue");
            Dust = data.Bool("dust");
            AttachToSolid = data.Bool("attachToSolid");
        }

        public override void UpdateTexture()
        {
            if (Dust)
            {
                // Dustbunny
                //return;
                Color = "Red";
            }

            string path = BasePath.Replace("{layer}", "fg").Replace("{color}", Color.ToLower());
            Texture = Atlas.Gameplay[path + "00"];
        }

        public void CreateBackgroundSpinners(List<Entity> entities, List<BackgroundSpinner> backgroundSpinners)
        {
            foreach (Entity entity in entities)
            {
                if (entity is Spinner otherSpinner)
                {
                    if (otherSpinner != this && AttachToSolid == otherSpinner.AttachToSolid && otherSpinner.Position.X >= Position.X && (otherSpinner.Position - Position).Length() < 24f)
                        backgroundSpinners.Add(new BackgroundSpinner(this, (Position + otherSpinner.Position) / 2f - Position));
                }
            }
        }
    }
}
