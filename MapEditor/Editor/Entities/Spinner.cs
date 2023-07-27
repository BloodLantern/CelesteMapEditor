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

            public BackgroundSpinner(EntityData data, Level level, Vector2 offset) : base(data, level)
            {
                Offset = offset;
            }

            public override void UpdateTexture()
            {
                if (Dust)
                {
                    // Dustbunny
                    //return;
                    Color = "Red";
                }

                string path = BasePath + "bg_" + Color.ToLower();
                Texture = Calc.Random.Choose(Atlas.Gameplay.GetAtlasSubtextures(path));
                Texture.Rotation = Calc.Random.Choose(0, 1, 2, 3) * MathHelper.PiOver2;
                Texture.Origin = Texture.Size.ToVector2() / 2;
            }
        }

        public const string BasePath = "danger/crystal/";

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

            string path = BasePath + "fg_" + Color.ToLower();
            Texture = Calc.Random.Choose(Atlas.Gameplay.GetAtlasSubtextures(path));
        }

        public void CreateBackgroundSpinners(List<Entity> entities, List<BackgroundSpinner> backgroundSpinners)
        {
            foreach (Entity entity in entities)
            {
                if (entity is Spinner otherSpinner)
                {
                    if (otherSpinner != this && AttachToSolid == otherSpinner.AttachToSolid && otherSpinner.Position.X >= Position.X && (otherSpinner.Position - Position).Length() < 24f)
                        backgroundSpinners.Add(new BackgroundSpinner(EntityData, Level, (Position + otherSpinner.Position) / 2f - Position));
                }
            }
        }
    }
}
